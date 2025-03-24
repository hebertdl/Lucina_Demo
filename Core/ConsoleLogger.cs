using Core.Interfaces;

namespace Core;

public class ConsoleLogger : ILogger
{
    public void LogInfo(string logIdentifier, string message)
    {
        Console.WriteLine($"Info: {DateTime.UtcNow}: {logIdentifier}: {message}");
    }

    public void LogError(string logIdentifier, string message, Exception ex)
    {
        Console.WriteLine($"Error: {DateTime.UtcNow}: {logIdentifier}: {message} Error: {ex?.Message ?? ""}");
    }

    public void LogValidationError(string logIdentifier, string validationMessage, string data)
    {
        Console.WriteLine($"Validation Error: {DateTime.UtcNow}: {logIdentifier}: {validationMessage} Data: {data}");
    }
}