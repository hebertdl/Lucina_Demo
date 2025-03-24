using Collectors.Interfaces;
using Core.Interfaces;
using Quartz;
using IJob = Quartz.IJob;

namespace Collectors;

[DisallowConcurrentExecution]
public class BatchProcessingJob(
    IBatchProcessor batchProcessor,
    IDataProcessor dataProcessor,
    IFileStorage fileStorage,
    ILogger logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("BatchProcessingJob executing: " + DateTime.UtcNow);
        await batchProcessor.RunBatchProcessAsync(dataProcessor, fileStorage, logger);
        Console.WriteLine("BatchProcessingJob completed: " + DateTime.UtcNow);
    }
}