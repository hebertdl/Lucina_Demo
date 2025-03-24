using Collectors.Interfaces;
using Core.Interfaces;
using ILogger = Core.Interfaces.ILogger;

namespace Collectors;

public class BatchProcessor(BatchPusher batchPusher) : IBatchProcessor
{
    private const string LogIdentifier = "BatchProcessor";

    public async Task RunBatchProcessAsync(IDataProcessor processor, IFileStorage storage, ILogger logger)
    {
        const string postUrl = "https://reqres.in/api/users";
        var date = DateTime.UtcNow.AddYears(-1).AddDays(-1); // Yesterday's date

        try
        {
            logger.LogInfo(LogIdentifier, "Batch processing started");
            var processedJsonData = await processor.ExecuteDataProcessor(date);
            await batchPusher.PostResults(postUrl, processedJsonData, logger);
            logger.LogInfo(LogIdentifier, "Batch processing completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(LogIdentifier, $"Batch processing error: {ex.StackTrace}", ex);
            throw;
        }
    }
}