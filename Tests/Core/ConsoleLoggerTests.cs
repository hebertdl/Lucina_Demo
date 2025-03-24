using Core;

namespace Lucina_Demo_Tests.Core;

public class ConsoleLoggerTests : IDisposable
{
    private readonly ConsoleLogger _logger;
    private readonly TextWriter _originalConsoleOut;
    private readonly StringWriter _stringWriter;

    public ConsoleLoggerTests()
    {
        _logger = new ConsoleLogger();
        _stringWriter = new StringWriter();
        _originalConsoleOut = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_originalConsoleOut);
        _stringWriter.Dispose();
    }

    [Fact]
    public void LogInfo_WritesCorrectFormat()
    {
        // Arrange
        var logIdentifier = "TestIdentifier";
        var message = "Test message";
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        _logger.LogInfo(logIdentifier, message);

        // Assert
        var output = _stringWriter.ToString().Trim();
        Assert.StartsWith("Info: ", output);
        Assert.Contains($": {logIdentifier}: {message}", output);

        var timestampStr = output.Split(": ")[1].Split($": {logIdentifier}")[0];
        var timestamp = DateTime.Parse(timestampStr);
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.True(timestamp >= before && timestamp <= after,
            $"Timestamp {timestamp} should be between {before} and {after}");
    }

    [Fact]
    public void LogError_WritesCorrectFormat_WithException()
    {
        // Arrange
        var logIdentifier = "ErrorIdentifier";
        var message = "Error occurred";
        var exception = new InvalidOperationException("Operation failed");
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        _logger.LogError(logIdentifier, message, exception);

        // Assert
        var output = _stringWriter.ToString().Trim();
        Assert.StartsWith("Error: ", output);
        Assert.Contains($": {logIdentifier}: {message} Error: {exception.Message}", output);

        var timestampStr = output.Split(": ")[1].Split($": {logIdentifier}")[0];
        var timestamp = DateTime.Parse(timestampStr);
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.True(timestamp >= before && timestamp <= after,
            $"Timestamp {timestamp} should be between {before} and {after}");
    }

    [Fact]
    public void LogValidationError_WritesCorrectFormat_WithData()
    {
        // Arrange
        var logIdentifier = "ValidationIdentifier";
        var validationMessage = "Invalid data detected";
        var data = "{\"key\":\"value\"}";
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        _logger.LogValidationError(logIdentifier, validationMessage, data);

        // Assert
        var output = _stringWriter.ToString().Trim();
        Assert.StartsWith("Validation Error: ", output);
        Assert.Contains($": {logIdentifier}: {validationMessage} Data: {data}", output);

        var timestampStr = output.Split(": ")[1].Split($": {logIdentifier}")[0];
        var timestamp = DateTime.Parse(timestampStr);
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.True(timestamp >= before && timestamp <= after,
            $"Timestamp {timestamp} should be between {before} and {after}");
    }

    [Fact]
    public void LogInfo_WithEmptyMessage_WritesEmptyMessage()
    {
        // Arrange
        var logIdentifier = "EmptyMessageTest";
        var message = "";

        var before = DateTime.UtcNow.AddSeconds(-1);
        // Act
        _logger.LogInfo(logIdentifier, message);

        // Assert
        var output = _stringWriter.ToString().Trim();
        var timestampStr = output.Split(": ")[1].Split($": {logIdentifier}")[0];
        Assert.Equal($"Info: {timestampStr}: {logIdentifier}:", output);

        var timestamp = DateTime.Parse(timestampStr);
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.True(timestamp >= before && timestamp <= after,
            $"Timestamp {timestamp} should be between {before} and {after}");
    }

    [Fact]
    public void LogError_WithNullException_WritesWithoutExceptionMessage()
    {
        // Arrange
        var logIdentifier = "NullExceptionTest";
        var message = "Error with no exception";
        Exception? ex = null;
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        _logger.LogError(logIdentifier, message, ex);

        // Assert
        var output = _stringWriter.ToString().Trim();
        Console.WriteLine($"Full output: '{output}'"); // Debug output
        Assert.StartsWith("Error: ", output);
        Assert.Contains($": {logIdentifier}: {message} Error:", output);
        Assert.DoesNotContain("Error: null", output);

        var timestampStr = output.Split(": ")[1].Split($": {logIdentifier}")[0];
        var timestamp = DateTime.Parse(timestampStr);
        var after = DateTime.UtcNow.AddSeconds(1);
        Assert.True(timestamp >= before && timestamp <= after,
            $"Timestamp {timestamp} should be between {before} and {after}");
    }
}