using System.Reflection;
using Core;

namespace Lucina_Demo_Tests.Core;

public class LocalFileStorageTests
{
    private const string DataFolder = "Data";
    private readonly LocalFileStorage _fileStorage;

    public LocalFileStorageTests()
    {
        _fileStorage = new LocalFileStorage();
        // Ensure the Data folder exists for testing
        Directory.CreateDirectory(DataFolder);
    }

    [Fact]
    public async Task SaveDataFile_WritesDataToCorrectFile()
    {
        // Arrange
        var filePrefix = "test";
        var date = new DateTime(2025, 03, 23);
        var data = "test content";
        var expectedFilePath = Path.Combine(DataFolder, "test_03232025.json");

        // Act
        await _fileStorage.SaveDataFile(filePrefix, date, data);

        // Assert
        Assert.True(File.Exists(expectedFilePath));
        var fileContent = await File.ReadAllTextAsync(expectedFilePath);
        Assert.Equal(data, fileContent);

        // Cleanup
        File.Delete(expectedFilePath);
    }

    [Fact]
    public async Task GetDataFileContents_ReturnsCorrectContents()
    {
        // Arrange
        var filePrefix = "test";
        var date = new DateTime(2025, 03, 23);
        var data = "test content";
        var filePath = Path.Combine(DataFolder, "test_03232025.json");
        await File.WriteAllTextAsync(filePath, data);

        // Act
        using var resultStream = await _fileStorage.GetDataFileContents(filePrefix, date);
        using var reader = new StreamReader(resultStream);
        var result = await reader.ReadToEndAsync();

        // Assert
        Assert.Equal(data, result);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task GetDataFileContents_ResetsStreamPositionToZero()
    {
        // Arrange
        var filePrefix = "test";
        var date = new DateTime(2025, 03, 23);
        var data = "test content";
        var filePath = Path.Combine(DataFolder, "test_03232025.json");
        await File.WriteAllTextAsync(filePath, data);

        // Act
        using var resultStream = await _fileStorage.GetDataFileContents(filePrefix, date);

        // Assert
        Assert.Equal(0, resultStream.Position);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task SaveDataFile_OverwritesExistingFile()
    {
        // Arrange
        var filePrefix = "test";
        var date = new DateTime(2025, 03, 23);
        var initialData = "initial content";
        var newData = "new content";
        var filePath = Path.Combine(DataFolder, "test_03232025.json");
        await File.WriteAllTextAsync(filePath, initialData);

        // Act
        await _fileStorage.SaveDataFile(filePrefix, date, newData);

        // Assert
        var fileContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(newData, fileContent);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void Filename_GeneratesCorrectPath()
    {
        // Arrange
        var filePrefix = "test";
        var date = new DateTime(2025, 03, 23);
        var expectedPath = Path.Combine(DataFolder, "test_03232025.json");

        // Act
        var result = typeof(LocalFileStorage)
            .GetMethod("Filename", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.Invoke(_fileStorage, [filePrefix, date])
            ?.ToString();

        // Assert
        Assert.Equal(expectedPath, result);
    }
}