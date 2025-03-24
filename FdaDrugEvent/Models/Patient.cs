using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class Patient
{
    [JsonProperty("patientonsetage")] public string? PatientOnsetAge { get; set; } // Age at onset (e.g., "72")

    [JsonProperty("patientonsetageunit")]
    public string? PatientOnsetAgeUnit { get; set; } // Unit of age: 801=Years, 802=Months, etc.

    [JsonProperty("patientagegroup")]
    public string? PatientAgeGroup { get; set; } // Age group: 1=Neonate, 2=Infant, ..., 6=Elderly

    [JsonProperty("patientweight")] public string? PatientWeightKg { get; set; } // Weight in kg (e.g., "93.8")

    [JsonProperty("patientsex")] public string? PatientSex { get; set; } // Sex: 1=Male, 2=Female, 0=Unknown

    [JsonProperty("reaction")] public List<Reaction>? Reactions { get; set; } // List of adverse reactions

    [JsonProperty("drug")] public List<Drug>? Drugs { get; set; } // List of associated drugs

    [JsonProperty("summary")] public Summary? Summary { get; set; } // Case summary
}