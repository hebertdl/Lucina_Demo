namespace Core.Interfaces;

public interface ILogger
{
    void LogInfo(string logIdentifier, string message);
    void LogError(string logIdentifier, string message, Exception ex);
    void LogValidationError(string logIdentifier, string validationMessage, string data);
}