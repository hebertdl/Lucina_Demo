using Collectors;
using Core.Interfaces;
using Moq;
using Quartz;
using Quartz.Spi;

namespace Lucina_Demo_Tests.Collectors;

public class MicrosoftDependencyInjectionJobFactoryTests
{
    private readonly MicrosoftDependencyInjectionJobFactory _jobFactory;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<IServiceProvider> _mockServiceProvider;

    public MicrosoftDependencyInjectionJobFactoryTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger>();
        _jobFactory = new MicrosoftDependencyInjectionJobFactory(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public void NewJob_ResolvesJob_ReturnsJobInstance()
    {
        // Arrange
        var jobType = typeof(BatchProcessingJob);
        var mockJob = new Mock<IJob>().Object;
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.Setup(j => j.JobType).Returns(jobType);
        var dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);

        var bundle = new TriggerFiredBundle(jobDetail.Object, null, null, false, dateTimeOffset, null, null, null);
        var scheduler = new Mock<IScheduler>();

        _mockServiceProvider.Setup(sp => sp.GetService(jobType)).Returns(mockJob);

        // Act
        var result = _jobFactory.NewJob(bundle, scheduler.Object);

        // Assert
        Assert.Equal(mockJob, result);
        _mockLogger.Verify(
            l => l.LogInfo("MicrosoftDependencyInjectionJobFactory", $"Creating job instance: {jobType.Name}"),
            Times.Once());
        _mockLogger.Verify(
            l => l.LogInfo("MicrosoftDependencyInjectionJobFactory",
                It.Is<string>(s => s.StartsWith("Job instance created: "))), Times.Once());
    }

    [Fact]
    public void NewJob_ServiceNotResolved_ThrowsInvalidOperationException()
    {
        // Arrange
        var jobType = typeof(BatchProcessingJob);
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.Setup(j => j.JobType).Returns(jobType);
        var dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);

        var bundle = new TriggerFiredBundle(jobDetail.Object, null, null, false, dateTimeOffset, null, null, null);
        var scheduler = new Mock<IScheduler>();

        _mockServiceProvider.Setup(sp => sp.GetService(jobType)).Returns(null); // Simulate failure to resolve

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _jobFactory.NewJob(bundle, scheduler.Object));
        Assert.Equal($"No service for type '{jobType}' has been registered.", ex.Message);
        _mockLogger.Verify(
            l => l.LogInfo("MicrosoftDependencyInjectionJobFactory", $"Creating job instance: {jobType.Name}"),
            Times.Once());
        _mockLogger.Verify(
            l => l.LogError("MicrosoftDependencyInjectionJobFactory",
                It.Is<string>(s => s.StartsWith("Error creating job instance: ")), It.IsAny<Exception>()),
            Times.Once());
    }

    [Fact]
    public void NewJob_ExceptionDuringResolution_LogsAndRethrows()
    {
        // Arrange
        var jobType = typeof(BatchProcessingJob);
        var jobDetail = new Mock<IJobDetail>();
        jobDetail.Setup(j => j.JobType).Returns(jobType);
        var dateTimeOffset = new DateTimeOffset(DateTime.UtcNow);

        var bundle = new TriggerFiredBundle(jobDetail.Object, null, null, false, dateTimeOffset, null, null, null);
        var scheduler = new Mock<IScheduler>();
        var exception = new InvalidOperationException("DI failure");

        _mockServiceProvider.Setup(sp => sp.GetService(jobType)).Throws(exception);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _jobFactory.NewJob(bundle, scheduler.Object));
        Assert.Equal(exception, ex);
        _mockLogger.Verify(
            l => l.LogInfo("MicrosoftDependencyInjectionJobFactory", $"Creating job instance: {jobType.Name}"),
            Times.Once());
        _mockLogger.Verify(
            l => l.LogError("MicrosoftDependencyInjectionJobFactory",
                It.Is<string>(s => s.Contains("Error creating job instance")), exception), Times.Once());
    }

    [Fact]
    public void ReturnJob_DoesNothing()
    {
        // Arrange
        var mockJob = new Mock<IJob>().Object;

        // Act
        _jobFactory.ReturnJob(mockJob);

        // Assert
        // No exceptions or behavior to verify; just ensure it doesn't throw
    }
}