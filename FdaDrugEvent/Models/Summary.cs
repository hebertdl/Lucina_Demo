using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class Summary
{
    [JsonProperty("narrativeincludeclinical")]
    public string NarrativeSummary { get; set; } = ""; // Narrative summary (e.g., "CASE EVENT DATE: 20240313")
}