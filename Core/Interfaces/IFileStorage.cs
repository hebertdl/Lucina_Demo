namespace Core.Interfaces;

public interface IFileStorage
{
    Task SaveDataFile(string filePrefix, DateTime date, string data);
    Task<MemoryStream> GetDataFileContents(string filePrefix, DateTime date);
}