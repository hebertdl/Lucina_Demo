using Collectors;
using Collectors.Interfaces;
using Core.Interfaces;
using JetBrains.Annotations;
using Moq;
using Quartz;

namespace Lucina_Demo_Tests.Collectors;

[TestSubject(typeof(BatchProcessingJob))]
public class BatchProcessingJobTests
{
    private readonly BatchProcessingJob _job;
    private readonly Mock<IBatchProcessor> _mockBatchProcessor;
    private readonly Mock<IDataProcessor> _mockDataProcessor;
    private readonly Mock<IFileStorage> _mockFileStorage;
    private readonly Mock<IJobExecutionContext> _mockJobExecutionContext;
    private readonly Mock<ILogger> _mockLogger;

    public BatchProcessingJobTests()
    {
        _mockBatchProcessor = new Mock<IBatchProcessor>();
        _mockDataProcessor = new Mock<IDataProcessor>();
        _mockFileStorage = new Mock<IFileStorage>();
        _mockLogger = new Mock<ILogger>();
        _mockJobExecutionContext = new Mock<IJobExecutionContext>();
        _job = new BatchProcessingJob(_mockBatchProcessor.Object, _mockDataProcessor.Object, _mockFileStorage.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Execute_CallsRunBatchProcessAsync_WithCorrectDependencies()
    {
        // Arrange
        _mockBatchProcessor.Setup(bp =>
                bp.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object, _mockLogger.Object))
            .Returns(Task.CompletedTask);

        // Act
        await _job.Execute(_mockJobExecutionContext.Object);

        // Assert
        _mockBatchProcessor.Verify(
            bp => bp.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object, _mockLogger.Object),
            Times.Once());
    }

    [Fact]
    public async Task Execute_WhenBatchProcessorThrowsException_PropagatesException()
    {
        // Arrange
        var exception = new InvalidOperationException("Batch process failed");
        _mockBatchProcessor.Setup(bp =>
                bp.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object, _mockLogger.Object))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _job.Execute(_mockJobExecutionContext.Object));
        Assert.Equal(exception, thrownException);
        _mockBatchProcessor.Verify(
            bp => bp.RunBatchProcessAsync(_mockDataProcessor.Object, _mockFileStorage.Object, _mockLogger.Object),
            Times.Once());
    }
}