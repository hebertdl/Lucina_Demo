using Core.Interfaces;

namespace Core;

public class LocalFileStorage : IFileStorage
{
    private string DataFolder { get; } = "Data";

    public async Task SaveDataFile(string filePrefix, DateTime date, string data)
    {
        EnsureDataFolderExists();
        await File.WriteAllTextAsync(Filename(filePrefix, date), data);
    }

    public async Task<MemoryStream> GetDataFileContents(string filePrefix, DateTime date)
    {
        EnsureDataFolderExists();
        var filePath = Filename(filePrefix, date);
        var memoryStream = new MemoryStream();
        await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            await fileStream.CopyToAsync(memoryStream);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private string Filename(string filePrefix, DateTime date)
    {
        return Path.Combine(DataFolder, $"{filePrefix}_{date:MMddyyyy}.json");
    }

    private void EnsureDataFolderExists()
    {
        if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
    }
}