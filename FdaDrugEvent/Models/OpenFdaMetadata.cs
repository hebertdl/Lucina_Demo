using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class OpenFdaMetadata
{
    [JsonProperty("application_number")]
    public List<string>? FdaApplicationNumber { get; set; } // FDA application number (e.g., ["BLA761028"])

    [JsonProperty("brand_name")] public List<string>? BrandNames { get; set; } // Brand names (e.g., ["MVASI"])

    [JsonProperty("generic_name")]
    public List<string>? GenericNames { get; set; } // Generic names (e.g., ["BEVACIZUMAB-AWWB"])

    [JsonProperty("manufacturer_name")]
    public List<string>? ManufacturerNames { get; set; } // Manufacturers (e.g., ["Amgen Inc"])

    [JsonProperty("product_ndc")]
    public List<string>? ProductNdcCodes { get; set; } // Product NDC codes (e.g., ["55513-206"])

    [JsonProperty("product_type")]
    public List<string>? ProductTypes { get; set; } // Product type (e.g., ["HUMAN PRESCRIPTION DRUG"])

    [JsonProperty("route")]
    public List<string>? AdministrationRoutes { get; set; } // Administration routes (e.g., ["INTRAVENOUS"])

    [JsonProperty("substance_name")]
    public List<string>? SubstanceNames { get; set; } // Substance names (e.g., ["BEVACIZUMAB"])

    [JsonProperty("package_ndc")]
    public List<string>? PackageNdcCodes { get; set; } // Package NDC codes (e.g., ["55513-206-01"])

    [JsonProperty("nui")]
    public List<string>? NlmUniqueIdentifiers { get; set; } // NLM Unique Identifiers (e.g., ["N0000193543"])

    [JsonProperty("pharm_class_epc")]
    public List<string>?
        PharmacyClassEstablishedClasses
    {
        get;
        set;
    } // Established Pharmacologic Class (e.g., ["Vascular Endothelial Growth Factor Inhibitor [EPC]"])

    [JsonProperty("pharm_class_moa")]
    public List<string>?
        PharmacyClassMechanismOfActions
    {
        get;
        set;
    } // Mechanism of Action (e.g., ["Vascular Endothelial Growth Factor Inhibitors [MoA]"])

    [JsonProperty("unii")]
    public List<string>? UniqueIdentifiers { get; set; } // Unique Ingredient Identifiers (e.g., ["2S9ZZM9Q9V"])
}