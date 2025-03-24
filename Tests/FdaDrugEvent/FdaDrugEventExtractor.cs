using Core.Interfaces;
using FdaDrugEvent;
using Moq;
using Newtonsoft.Json;

namespace Lucina_Demo_Tests.FdaDrugEvent;

public class FdaDrugEventExtractorTests
{
    private readonly FdaDrugEventExtractor _extractor;
    private readonly Mock<ILogger> _mockLogger;

    public FdaDrugEventExtractorTests()
    {
        _mockLogger = new Mock<ILogger>();
        _extractor = new FdaDrugEventExtractor(_mockLogger.Object);
    }

    [Fact]
    public async Task ConvertToFdaEvents_ValidRawData_ReturnsFdaEventsWithPatients()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);
        var rawData = @"{
                ""results"": [{
                    ""patient"": {
                        ""patientonsetage"": ""72"",
                        ""patientonsetageunit"": ""801"",
                        ""patientagegroup"": ""6"",
                        ""patientweight"": ""93.8"",
                        ""patientsex"": ""2"",
                        ""reaction"": [{""reactionmeddrapt"": ""Headache"", ""reactionoutcome"": ""1""}],
                        ""drug"": [{
                            ""drugcharacterization"": ""1"",
                            ""medicinalproduct"": ""MVASI"",
                            ""drugindication"": ""Colorectal cancer""
                        }],
                        ""summary"": {""narrativeincludeclinical"": ""CASE EVENT DATE: 20230323""}
                    }
                }]
            }";
        var before = DateTime.UtcNow;

        // Act
        var result = await _extractor.ConvertToFdaEvents(rawData, reportDate);

        // Assert
        var after = DateTime.UtcNow;
        Assert.NotNull(result);
        Assert.Equal(reportDate, result.ReportDate);
        Assert.True(result.ProcessDate >= before && result.ProcessDate <= after);
        Assert.Equal(1, result.TotalRecords);
        Assert.NotNull(result.Patients);
        Assert.Equal(1, result.Patients.Count);

        var patient = result.Patients[0];
        Assert.Equal("72", patient.PatientOnsetAge);
        Assert.Equal("801", patient.PatientOnsetAgeUnit);
        Assert.Equal("6", patient.PatientAgeGroup);
        Assert.Equal("93.8", patient.PatientWeightKg);
        Assert.Equal("2", patient.PatientSex);

        Assert.NotNull(patient.Reactions);
        Assert.Equal(1, patient.Reactions.Count);
        Assert.Equal("Headache", patient.Reactions[0].ReactionMeddraPt);
        Assert.Equal("1", patient.Reactions[0].ReactionOutcome);

        Assert.NotNull(patient.Drugs);
        Assert.Equal(1, patient.Drugs.Count);
        Assert.Equal("1", patient.Drugs[0].DrugReactionRole);
        Assert.Equal("MVASI", patient.Drugs[0].DrugName);
        Assert.Equal("Colorectal cancer", patient.Drugs[0].DrugIndication);

        Assert.NotNull(patient.Summary);
        Assert.Equal("CASE EVENT DATE: 20230323", patient.Summary.NarrativeSummary);
    }

    [Fact]
    public async Task ConvertToFdaEvents_NullRawData_ThrowsArgumentNullException()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _extractor.ConvertToFdaEvents(null!, reportDate));
        Assert.Equal("rawData", ex.ParamName);
        _mockLogger.Verify(
            l => l.LogError("Convert To FDA Events:", "", It.Is<ArgumentNullException>(e => e.ParamName == "rawData")),
            Times.Once());
    }

    [Fact]
    public async Task ConvertToFdaEvents_EmptyRawData_ThrowsArgumentNullException()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _extractor.ConvertToFdaEvents("", reportDate));
        Assert.Equal("rawData", ex.ParamName);
        _mockLogger.Verify(
            l => l.LogError("Convert To FDA Events:", "", It.Is<ArgumentNullException>(e => e.ParamName == "rawData")),
            Times.Once());
    }

    [Fact]
    public async Task ConvertToFdaEvents_InvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);
        var rawData = "invalid json";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _extractor.ConvertToFdaEvents(rawData, reportDate));
        Assert.Contains("Failed to parse raw FDA data JSON", ex.Message);
        Assert.IsType<JsonReaderException>(ex.InnerException);
    }

    [Fact]
    public async Task ConvertToFdaEvents_NoResults_ReturnsEmptyPatients()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);
        var rawData = "{\"meta\":{\"results\":{\"total\":0}}}";

        // Act
        var result = await _extractor.ConvertToFdaEvents(rawData, reportDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(reportDate, result.ReportDate);
        Assert.Equal(0, result.TotalRecords);
        Assert.Null(result.Patients);
    }

    [Fact]
    public async Task ConvertToFdaEvents_InvalidPatientJson_LogsValidationError()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);
        var rawData = "{\"results\":[{\"patient\":\"invalid json\"}]}";
        var expectedLogData =
            "{\n  \"results\": [\n    {\n      \"patient\": \"invalid json\"\n    }\n  ]\n}"
                .Replace("\r\n", "\n"); // Normalize to \n

        // Act
        var result = await _extractor.ConvertToFdaEvents(rawData, reportDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalRecords);
        Assert.Null(result.Patients);
        _mockLogger.Verify(
            l => l.LogValidationError("Extract Patients", "Failed to deserialize patient.", It.IsAny<string>()),
            Times.Once());
    }

    [Fact]
    public async Task ConvertToFdaEvents_MissingPatientData_ReturnsPartialPatients()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);
        var rawData = @"{
                ""results"": [
                    {
                        ""patient"": {
                            ""patientonsetage"": ""45"",
                            ""patientsex"": ""1"",
                            ""drug"": [{""medicinalproduct"": ""ASPIRIN""}]
                        }
                    },
                    {""no_patient"": true}
                ]
            }";

        // Act
        var result = await _extractor.ConvertToFdaEvents(rawData, reportDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalRecords);
        Assert.NotNull(result.Patients);
        Assert.Equal(1, result.Patients.Count);
        Assert.Equal("45", result.Patients[0].PatientOnsetAge);
        Assert.Equal("1", result.Patients[0].PatientSex);
        Assert.NotNull(result.Patients[0].Drugs);
        Assert.Equal("ASPIRIN", result.Patients[0].Drugs[0].DrugName);
    }

    [Fact]
    public async Task ConvertToFdaEvents_WithOpenFdaMetadata_ParsesCorrectly()
    {
        // Arrange
        var reportDate = new DateTime(2023, 3, 23);
        var rawData = @"{
                ""results"": [{
                    ""patient"": {
                        ""patientonsetage"": ""30"",
                        ""drug"": [{
                            ""medicinalproduct"": ""MVASI"",
                            ""openfda"": {
                                ""brand_name"": [""MVASI""],
                                ""generic_name"": [""BEVACIZUMAB-AWWB""]
                            }
                        }]
                    }
                }]
            }";

        // Act
        var result = await _extractor.ConvertToFdaEvents(rawData, reportDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalRecords);
        Assert.NotNull(result.Patients);
        Assert.Equal("30", result.Patients[0].PatientOnsetAge);
        Assert.NotNull(result.Patients[0].Drugs);
        Assert.Equal("MVASI", result.Patients[0].Drugs[0].DrugName);
        Assert.NotNull(result.Patients[0].Drugs[0].OpenFdaMetadata);
        Assert.Contains("MVASI", result.Patients[0].Drugs[0].OpenFdaMetadata.BrandNames);
        Assert.Contains("BEVACIZUMAB-AWWB", result.Patients[0].Drugs[0].OpenFdaMetadata.GenericNames);
    }
}