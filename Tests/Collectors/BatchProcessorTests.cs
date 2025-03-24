using System.Net;
using Collectors;
using Core.Interfaces;
using Moq;
using Moq.Protected;

namespace Lucina_Demo_Tests.Collectors;

public class BatchProcessorTests
{
    private readonly BatchPusher _batchPusher;
    private readonly Mock<IDataProcessor> _mockDataProcessor;
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger> _mockLogger;

    public BatchProcessorTests()
    {
        _mockDataProcessor = new Mock<IDataProcessor>();
        _mockFileStorage = new Mock<IFileStorage>();
        _mockLogger = new Mock<ILogger>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _batchPusher = new BatchPusher(httpClient, new Mock<IAuthHeaderBuilder>().Object);
    }

    [Fact]
    public async Task RunBatchProcessAsync_SuccessfulRun_LogsStartAndComplete()
    {
        // Arrange
        var batchProcessor = new BatchProcessor(_batchPusher);
        _mockDataProcessor.Setup(p => p.ExecuteDataProcessor(It.IsAny<DateTime>()))
            .ReturnsAsync("{\"name\": \"John Doe\", \"job\": \"Developer\"}");
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent("{\"id\": \"123\", \"name\": \"John Doe\", \"job\": \"Developer\"}")
            });

        // Act
        await batchProcessor.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object,
            _mockLogger.Object);

        // Assert
        _mockLogger.Verify(l => l.LogInfo("BatchProcessor", "Batch processing started"), Times.Once());
        _mockLogger.Verify(l => l.LogInfo("BatchProcessor", "Batch processing completed successfully"), Times.Once());
        _mockLogger.Verify(l => l.LogError(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Exception>()),
            Times.Never());
        _mockDataProcessor.Verify(p => p.ExecuteDataProcessor(It.Is<DateTime>(d =>
            d.Year == DateTime.UtcNow.AddYears(-1).Year && d.Day == DateTime.UtcNow.AddDays(-1).Day)), Times.Once());
    }

    [Fact]
    public async Task RunBatchProcessAsync_ProcessorThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var batchProcessor = new BatchProcessor(_batchPusher);
        var exception = new InvalidOperationException("Processor failed");
        _mockDataProcessor.Setup(p => p.ExecuteDataProcessor(It.IsAny<DateTime>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            batchProcessor.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object,
                _mockLogger.Object));
        Assert.Same(exception, thrownException);
        _mockLogger.Verify(l => l.LogInfo("BatchProcessor", "Batch processing started"), Times.Once());
        _mockLogger.Verify(
            l => l.LogError("BatchProcessor", It.Is<string>(s => s.Contains("Batch processing error")), exception),
            Times.Once());
        _mockLogger.Verify(l => l.LogInfo("BatchProcessor", "Batch processing completed successfully"), Times.Never());
    }

    [Fact]
    public async Task RunBatchProcessAsync_PostFails_LogsErrorAndRethrows()
    {
        // Arrange
        var batchProcessor = new BatchProcessor(_batchPusher);
        _mockDataProcessor.Setup(p => p.ExecuteDataProcessor(It.IsAny<DateTime>()))
            .ReturnsAsync("{\"name\": \"John Doe\", \"job\": \"Developer\"}");
        var httpException = new HttpRequestException("Network failure");
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(httpException);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<HttpRequestException>(() =>
            batchProcessor.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object,
                _mockLogger.Object));
        Assert.Same(httpException, thrownException);
        _mockLogger.Verify(l => l.LogInfo("BatchProcessor", "Batch processing started"), Times.Once());
        _mockLogger.Verify(
            l => l.LogError("BatchProcessor", It.Is<string>(s => s.Contains("Batch processing error")), httpException),
            Times.Once());
        _mockLogger.Verify(l => l.LogInfo("BatchProcessor", "Batch processing completed successfully"), Times.Never());
    }
}