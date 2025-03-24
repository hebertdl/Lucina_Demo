using System.Net.Http.Headers;
using System.Text;
using Core.Interfaces;

namespace Collectors;

public class BatchPusher(HttpClient httpClient, IAuthHeaderBuilder authHeaderBuilder)
{
    private const string LogIdentifier = "BatchPusher";

    public async Task PostResults(string postUrl, string jsonData, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (string.IsNullOrEmpty(jsonData))
        {
            logger.LogError(LogIdentifier, "JSON data is null or empty", new ArgumentNullException(nameof(jsonData)));
            throw new ArgumentNullException(nameof(jsonData));
        }

        try
        {
            logger.LogInfo(LogIdentifier, $"Streaming data to {postUrl}");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData));
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpClient.DefaultRequestHeaders.Authorization = await authHeaderBuilder.BuildAuthHeaderAsync();
            var response = await httpClient.PostAsync(postUrl, content);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            logger.LogInfo(LogIdentifier, $"Post successful. Response Body Length: {responseBody.Length}");
        }
        catch (Exception ex)
        {
            logger.LogError(LogIdentifier, $"Failed to post results: {postUrl} Error: {ex.Message}", ex);
            throw;
        }
    }
}