using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace Collectors;

public class MicrosoftDependencyInjectionJobFactory(IServiceProvider serviceProvider, ILogger logger) : IJobFactory
{
    private const string LogIdentifier = "MicrosoftDependencyInjectionJobFactory";

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        logger.LogInfo(LogIdentifier, "Creating job instance: " + bundle.JobDetail.JobType.Name);
        try
        {
            var job = serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
            logger.LogInfo(LogIdentifier, "Job instance created: " + DateTime.UtcNow);
            return job ?? throw new InvalidOperationException($"Unable to resolve job type {bundle.JobDetail.JobType}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogIdentifier, $"Error creating job instance: {ex.StackTrace}", ex);
            throw;
        }
    }

    public void ReturnJob(IJob job)
    {
        // not needed
    }
}