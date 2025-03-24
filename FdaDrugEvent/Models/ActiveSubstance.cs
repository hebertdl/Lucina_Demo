using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class ActiveSubstance
{
    [JsonProperty("activesubstancename")]
    public string? ActiveSubstanceName { get; set; } // Active ingredient (e.g., "BEVACIZUMAB-AWWB")
}