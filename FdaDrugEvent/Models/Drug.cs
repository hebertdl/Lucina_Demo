using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class Drug
{
    [JsonProperty("drugcharacterization")]
    public string? DrugReactionRole { get; set; } // Role: 1=Suspect, 2=Concomitant, 3=Interacting

    [JsonProperty("medicinalproduct")] public string? DrugName { get; set; } // Drug name (e.g., "MVASI")

    [JsonProperty("drugbatchnumb")] public string? DrugBatchNumber { get; set; } // Batch number (e.g., "1167840")

    [JsonProperty("drugauthorizationnumb")]
    public string? DrugAuthorizationNumber { get; set; } // Authorization number (e.g., "761028")

    [JsonProperty("drugdosagetext")]
    public string? DrugDosageText { get; set; } // Free-text dosage (e.g., "UNK, DOSE ORDERED: 450 MG")

    [JsonProperty("drugdosageform")]
    public string? DrugDosageForm { get; set; } // Dosage form (e.g., "Solution for injection")

    [JsonProperty("drugadministrationroute")]
    public string? DrugAdministrationRoute { get; set; } // Route: 065=Intravenous, 048=Oral, etc.

    [JsonProperty("drugindication")]
    public string? DrugIndication { get; set; } // Indication (e.g., "Colorectal cancer")

    [JsonProperty("actiondrug")]
    public string? ActionTaken { get; set; } // Action taken: 1=Withdrawn, 2=Dose reduced, ..., 6=Unknown

    [JsonProperty("drugadditional")]
    public string? DrugAdditional { get; set; } // Additional info: 4=Biosimilar, 1=Compounded, etc.

    [JsonProperty("activesubstance")] public ActiveSubstance? ActiveSubstance { get; set; } // Active substance details

    [JsonProperty("openfda")] public OpenFdaMetadata? OpenFdaMetadata { get; set; } // OpenFDA metadata
}