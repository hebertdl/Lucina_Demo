using Core.Models;
using FdaDrugEvent.Interfaces;
using FdaDrugEvent.Patterns;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ILogger = Core.Interfaces.ILogger;

namespace FdaDrugEvent;

public class FdaDrugEventExtractor(ILogger logger) : IFdaDrugEventExtractor
{
    public Task<FdaEvents> ConvertToFdaEvents(string rawData, DateTime reportDate)
    {
        ValidateRawData(rawData);
        var processDate = DateTime.UtcNow;
        var json = ParseRawJson(rawData);
        var patients = ExtractPatients(json);
        var totalPatients = patients.Count;

        return Task.FromResult(CreateFdaEvents(processDate, reportDate, totalPatients, patients));
    }

    private void ValidateRawData(string rawData)
    {
        if (!string.IsNullOrEmpty(rawData)) return;
        var ex = new ArgumentNullException(nameof(rawData), "Raw FDA data cannot be null or empty.");
        logger.LogError("Convert To FDA Events:", "", ex);
        throw ex;
    }

    private static FdaEvents CreateFdaEvents(DateTime processDate, DateTime reportDate, int totalPatients,
        List<Patient> patients)
    {
        return new FdaEventsBuilder()
            .WithProcessDate(processDate)
            .WithReportDate(reportDate)
            .WithTotalRecords(totalPatients)
            .WithPatients(patients)
            .Build();
    }

    private List<Patient> ExtractPatients(JObject json)
    {
        var results = json["results"] as JArray;
        var patients = new List<Patient>();

        if (results == null) return patients;
        foreach (var result in results)
        {
            var patient = ExtractPatient(json, result, patients);
            if (patient != null) patients.Add(patient);
        }

        return patients;
    }

    private Patient? ExtractPatient(JObject json, JToken result, List<Patient> patients)
    {
        var patientJson = result["patient"]?.ToString();
        if (string.IsNullOrEmpty(patientJson)) return null;
        try
        {
            return JsonConvert.DeserializeObject<Patient>(patientJson);
        }
        catch (JsonException ex)
        {
            logger.LogValidationError("Extract Patients", "Failed to deserialize patient.", json.ToString());
            return null;
        }
    }

    private static JObject ParseRawJson(string rawData)
    {
        JObject json;
        try
        {
            json = JObject.Parse(rawData);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse raw FDA data JSON.", ex);
        }

        return json;
    }
}