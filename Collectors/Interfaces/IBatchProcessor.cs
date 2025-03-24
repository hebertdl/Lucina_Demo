using Core.Interfaces;

namespace Collectors.Interfaces;

public interface IBatchProcessor
{
    public Task RunBatchProcessAsync(IDataProcessor processor, IFileStorage storage, ILogger logger);
}