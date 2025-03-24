using Collectors.Interfaces;
using Core.Interfaces;
using Quartz;

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
        logger.LogInfo("Batch Processing Job", "BatchProcessingJob executing: " + DateTime.UtcNow);
        await batchProcessor.RunBatchProcessAsync(dataProcessor, fileStorage, logger);
        logger.LogInfo("Batch Processing Job", "BatchProcessingJob completed: " + DateTime.UtcNow);
    }
}