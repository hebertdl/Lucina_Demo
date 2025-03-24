using System.Diagnostics.CodeAnalysis;
using Collectors.Interfaces;
using Core;
using Core.Interfaces;
using FdaDrugEvent;
using FdaDrugEvent.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using ILogger = Core.Interfaces.ILogger;

namespace Collectors;

[ExcludeFromCodeCoverage]
internal static class Program
{
    private static ILogger _logger = new ConsoleLogger();

    private static async Task Main()
    {
        var services = new ServiceCollection();
        var serviceProvider = SetupServiceProvider(services);

        _logger = serviceProvider.GetRequiredService<ILogger>();
        LogInfo("Service provider built: " + DateTime.UtcNow);

        try
        {
            var scheduler = await InitializeQuartzScheduler(serviceProvider);
            var job = DefineTheBatchProcessingJob();
            var trigger = Define1AmTrigger();

            await scheduler.ScheduleJob(job, trigger);
            LogInfo("Job scheduled: " + DateTime.UtcNow);

            await scheduler.Start();
            LogInfo("Scheduler started: " + DateTime.UtcNow);
            LogInfo("Scheduler running: " + scheduler.IsStarted);

            // Manually trigger the job immediately for demonstration
            await scheduler.TriggerJob(job.Key);
            LogInfo("Manually triggered job: " + DateTime.UtcNow);

            LogInfo("Scheduler running. Executes at 1 AM UTC daily.");
            LogInfo("Press Ctrl+C to stop.");

            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex)
        {
            _logger.LogError("Main", $"Scheduler error: {ex.StackTrace}", ex);
        }
    }

    private static ITrigger Define1AmTrigger()
    {
        var trigger = TriggerBuilder.Create()
            .WithIdentity("batchTrigger", "batchGroup")
            .WithCronSchedule("0 0 1 * * ?", x => x.InTimeZone(TimeZoneInfo.Utc))
            .Build();
        LogInfo("Trigger defined: " + DateTime.UtcNow);
        LogInfo(
            $"Next scheduled run: {(trigger.GetNextFireTimeUtc().HasValue ? trigger.GetNextFireTimeUtc()!.Value.ToLocalTime() : "None")}");
        return trigger;
    }

    private static IJobDetail DefineTheBatchProcessingJob()
    {
        var job = JobBuilder.Create<BatchProcessingJob>()
            .WithIdentity("batchJob", "batchGroup")
            .Build();
        LogInfo("Job defined: " + DateTime.UtcNow);
        return job;
    }

    private static async Task<IScheduler> InitializeQuartzScheduler(ServiceProvider serviceProvider)
    {
        var schedulerFactory = new StdSchedulerFactory();
        var scheduler = await schedulerFactory.GetScheduler();
        LogInfo("Scheduler initialized: " + DateTime.UtcNow);
        scheduler.JobFactory = new MicrosoftDependencyInjectionJobFactory(serviceProvider, _logger);
        LogInfo("Job factory set: " + DateTime.UtcNow);
        return scheduler;
    }

    private static ServiceProvider SetupServiceProvider(ServiceCollection services)
    {
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }

    private static void LogInfo(string message)
    {
        _logger.LogInfo("Status: Main: ", message);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        const string clientId = "theClientId";
        const string clientSecret = "theSuperSecretSecret";
        const string tokenEndpoint = "https://example.com/token";

        services.AddHttpClient();
        services.AddTransient<IBatchProcessor, BatchProcessor>();
        services.AddTransient<IDataProcessor, FdaEventDataProcessor>();
        services.AddTransient<IFdaDrugEventExtractor, FdaDrugEventExtractor>(); // Added
        services.AddTransient<IFileStorage, LocalFileStorage>();
        services.AddTransient<ILogger, ConsoleLogger>();
        services.AddTransient<BatchProcessingJob>();
        services.AddTransient<BatchPusher>();

        services.AddTransient<IAuthHeaderBuilder>(provider =>
            new JwtAuthHeaderBuilder(
                provider.GetRequiredService<HttpClient>(),
                provider.GetRequiredService<ILogger>(),
                clientId,
                clientSecret,
                tokenEndpoint
            )
        );
    }
}