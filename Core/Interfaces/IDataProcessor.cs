namespace Core.Interfaces;

public interface IDataProcessor
{
    Task<string> ExecuteDataProcessor(DateTime date);
}