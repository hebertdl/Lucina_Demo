using Core.Interfaces;
using Core.Models;
using FdaDrugEvent.Filters;
using FdaDrugEvent.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FdaDrugEvent;

public class FdaEventDataProcessor(
    IHttpClientFactory httpClientFactory,
    IFileStorage fileStorage,
    ILogger logger,
    IFdaDrugEventExtractor fdaDrugEventExtractor)
    : IDataProcessor
{
    private const string BaseUrl = "https://api.fda.gov/drug/event.json";

    private readonly IFdaDrugEventExtractor _fdaDrugEventExtractor =
        fdaDrugEventExtractor ?? throw new ArgumentNullException(nameof(fdaDrugEventExtractor));

    private readonly IFileStorage
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage)); // Use injected instance

    private readonly IHttpClientFactory _httpClientFactory =
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private readonly ILogger
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Use injected instance

    public async Task<string> ExecuteDataProcessor(DateTime date)
    {
        var rawData = await GetBatchData(date);
        await _fileStorage.SaveDataFile("raw", date, rawData);
        var processedData = await ProcessedJsonData(rawData, date);
        var processedDataJson = JsonConvert.SerializeObject(processedData);
        await _fileStorage.SaveDataFile("processed", date, processedDataJson);
        var filteredData = ApplyDataFilters(processedData);
        return JsonConvert.SerializeObject(filteredData);
    }

    private static FdaEvents ApplyDataFilters(FdaEvents processedData)
    {
        var womanFilter = new FdaEventsWomanFilter();
        var first10Filter = new FdaEventsFirst10Filter();

        womanFilter.SetNext(first10Filter);

        return womanFilter.ApplyFilter(processedData);
    }

    private void LogInfo(string message)
    {
        _logger.LogInfo("Status: FDA Event Data Processor", message);
    }

    public async Task<string> GetBatchData(DateTime date)
    {
        var dateStr = date.ToString("yyyyMMdd");
        LogInfo($"Fetching all data for date: {date}");
        var allResults = new List<JToken>();
        var skip = 0;
        const int limit = 100;
        var total = 0;

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        do
        {
            var url = BuildUrl(dateStr, limit, skip);

            try
            {
                var jsonResponse = await GetJsonResponse(httpClient, url);
                var (meta, results) = ParseMetaAndResults(jsonResponse);

                if (meta == null || results == null)
                {
                    LogInfo("No data found in response.");
                    break;
                }

                total = meta["total"]?.Value<int>() ?? 0;
                allResults.AddRange(results);

                LogInfo($"Fetched {results.Count} records, skip={skip}, total={total}");
                skip += limit;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("FdaEventDataProcessor", "HTTP error fetching data", ex);
                throw;
            }
        } while (skip < total);

        var combinedResults = CombinedResultsMeta(allResults);

        LogInfo($"Total records fetched: {allResults.Count}");
        return combinedResults.ToString();
    }

    private static (JToken? meta, JArray? results) ParseMetaAndResults(string jsonResponse)
    {
        var json = JObject.Parse(jsonResponse);
        var meta = json["meta"]?["results"];
        var results = json["results"] as JArray;
        return (meta, results);
    }

    private static async Task<string> GetJsonResponse(HttpClient httpClient, string url)
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }

    private static JObject CombinedResultsMeta(List<JToken> allResults)
    {
        var combinedResults = new JObject
        {
            ["meta"] = new JObject
            {
                ["results"] = new JObject
                {
                    ["total"] = allResults.Count,
                    ["limit"] = allResults.Count,
                    ["skip"] = 0
                }
            },
            ["results"] = new JArray(allResults)
        };
        return combinedResults;
    }

    private string BuildUrl(string dateStr, int limit, int skip)
    {
        var url = $"{BaseUrl}?search=receivedate:{dateStr}&limit={limit}&skip={skip}";
        LogInfo($"Requesting: {url}");
        return url;
    }

    private async Task<FdaEvents> ProcessedJsonData(string rawData, DateTime date)
    {
        LogInfo("Processing data...");
        return await _fdaDrugEventExtractor.ConvertToFdaEvents(rawData, date);
    }
}