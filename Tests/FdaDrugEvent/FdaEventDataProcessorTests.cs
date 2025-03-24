using System.Net;
using System.Text;
using Core.Interfaces;
using Core.Models;
using FdaDrugEvent;
using FdaDrugEvent.Interfaces;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucina_Demo_Tests.FdaDrugEvent;

public class FdaEventDataProcessorTests
{
    private readonly Mock<IFdaDrugEventExtractor> _mockFdaEventExtractor;
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<ILogger> _mockLogger;
    private readonly FdaEventDataProcessor _processor;
    private readonly Mock<FdaEventDataProcessor> _processorMock; // Add this to hold the mock

    public FdaEventDataProcessorTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockFdaEventExtractor = new Mock<IFdaDrugEventExtractor>();
        _mockFileStorage = new Mock<IFileStorage>();
        _mockLogger = new Mock<ILogger>();
        _mockHttpHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        var httpClient = new HttpClient(_mockHttpHandler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Create a partial mock of FdaEventDataProcessor
        _processorMock = new Mock<FdaEventDataProcessor>(
            _mockHttpClientFactory.Object,
            _mockFileStorage.Object,
            _mockLogger.Object,
            _mockFdaEventExtractor.Object) { CallBase = true };

        _processor = _processorMock.Object; // Assign readonly field here
    }

    [Fact]
    public void Constructor_NullHttpClientFactory_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new FdaEventDataProcessor(null!, _mockFileStorage.Object, _mockLogger.Object,
                _mockFdaEventExtractor.Object));
        Assert.Equal("httpClientFactory", ex.ParamName);
    }

    [Fact]
    public void Constructor_NullFdaEventExtractor_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new FdaEventDataProcessor(_mockHttpClientFactory.Object, _mockFileStorage.Object, _mockLogger.Object,
                null!));
        Assert.Equal("fdaDrugEventExtractor", ex.ParamName);
    }

    [Fact]
    public async Task ExecuteDataProcessor_ValidData_SavesAndReturnsFilteredJson()
    {
        // Arrange
        var date = new DateTime(2023, 3, 23);
        var rawData = @"{
        ""meta"": {""results"": {""total"": 4, ""limit"": 4, ""skip"": 0}},
        ""results"": [
            {""patient"": {""patientonsetage"": ""72"", ""patientsex"": ""2""}},
            {""patient"": {""patientonsetage"": ""45"", ""patientsex"": ""1""}},
            {""patient"": {""patientonsetage"": ""30"", ""patientsex"": ""2""}},
            {""patient"": {""patientonsetage"": ""55"", ""patientsex"": ""2""}}
        ]
    }";

        var unfilteredFdaEvents = new FdaEvents
        {
            ProcessDate = date,
            ReportDate = date,
            TotalRecords = 4,
            Patients = new List<Patient>
            {
                new() { PatientOnsetAge = "72", PatientSex = "2" },
                new() { PatientOnsetAge = "45", PatientSex = "1" },
                new() { PatientOnsetAge = "30", PatientSex = "2" },
                new() { PatientOnsetAge = "55", PatientSex = "2" }
            }
        };

        var filteredPatients = unfilteredFdaEvents.Patients
            .Where(p => p.PatientSex == "2")
            .Take(10)
            .ToList();
        var expectedFilteredData = new FdaEvents
        {
            ProcessDate = date,
            ReportDate = date,
            TotalRecords = filteredPatients.Count,
            Patients = filteredPatients
        };
        var expectedFilteredJson = JsonConvert.SerializeObject(expectedFilteredData);

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(rawData, Encoding.UTF8, "application/json")
            });

        _mockFdaEventExtractor.Setup(e => e.ConvertToFdaEvents(It.IsAny<string>(), date))
            .Callback<string, DateTime>((s, d) =>
                Console.WriteLine($"ConvertToFdaEvents called with rawData: {s}, date: {d}"))
            .ReturnsAsync(unfilteredFdaEvents)
            .Verifiable();

        _mockFileStorage.Setup(f => f.SaveDataFile(It.IsAny<string>(), date, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _processor.ExecuteDataProcessor(date);

        // Assert
        Assert.Equal(expectedFilteredJson, result);
        _mockFileStorage.Verify(f => f.SaveDataFile("raw", date,
            It.Is<string>(s => JToken.DeepEquals(JToken.Parse(s), JToken.Parse(rawData)))), Times.Once());
        _mockFileStorage.Verify(f => f.SaveDataFile("processed", date, It.IsAny<string>()), Times.Once());

        _mockLogger.Verify(l => l.LogInfo("Status: FDA Event Data Processor", $"Fetching all data for date: {date}"),
            Times.Once());
        _mockLogger.Verify(l => l.LogInfo("Status: FDA Event Data Processor", "Processing data..."), Times.Once());
        _mockFdaEventExtractor.Verify();
    }

    [Fact]
    public async Task GetBatchData_NoData_ReturnsEmptyResults()
    {
        // Arrange
        var date = new DateTime(2023, 3, 23);
        var emptyResponse = "{\"meta\": {\"results\": {\"total\": 0}}}";

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(emptyResponse, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _processor.GetBatchData(date);

        // Assert
        var json = JObject.Parse(result);
        Assert.Equal(0, json["meta"]?["results"]?["total"]?.Value<int>());
        Assert.Empty(json["results"] as JArray ?? new JArray());
        _mockLogger.Verify(l => l.LogInfo("Status: FDA Event Data Processor", "No data found in response."),
            Times.Once());
    }

    [Fact]
    public async Task GetBatchData_HttpError_LogsAndThrows()
    {
        // Arrange
        var date = new DateTime(2023, 3, 23);
        var exception = new HttpRequestException("Network error");

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Throws(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _processor.GetBatchData(date));
        Assert.Equal(exception, ex);
        _mockLogger.Verify(l => l.LogError("FdaEventDataProcessor", "HTTP error fetching data", It.IsAny<Exception>()),
            Times.Once());
    }

    [Fact]
    public async Task GetBatchData_MultiplePages_CombinesResults()
    {
        // Arrange
        var date = new DateTime(2023, 3, 23);
        var firstPage =
            "{\"meta\": {\"results\": {\"total\": 150}}, \"results\": [{\"patient\": {\"id\": \"1\"}}, {\"patient\": {\"id\": \"2\"}}]}";
        var secondPage = "{\"meta\": {\"results\": {\"total\": 150}}, \"results\": [{\"patient\": {\"id\": \"3\"}}]}";

        _mockHttpHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(firstPage, Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(secondPage, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _processor.GetBatchData(date);

        // Assert
        var json = JObject.Parse(result);
        Assert.Equal(3, json["meta"]?["results"]?["total"]?.Value<int>());
        Assert.Equal(3, (json["results"] as JArray)?.Count);
        _mockLogger.Verify(l => l.LogInfo("Status: FDA Event Data Processor", "Fetched 2 records, skip=0, total=150"),
            Times.Once());
        _mockLogger.Verify(l => l.LogInfo("Status: FDA Event Data Processor", "Fetched 1 records, skip=100, total=150"),
            Times.Once());
        _mockLogger.Verify(l => l.LogInfo("Status: FDA Event Data Processor", "Total records fetched: 3"),
            Times.Once());
    }
}