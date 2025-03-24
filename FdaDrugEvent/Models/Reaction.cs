using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Core.Models;

[ExcludeFromCodeCoverage]
public class Reaction
{
    [JsonProperty("reactionmeddraversionpt")]
    public string ReactionMeddraVersionPt { get; set; } // MedDRA version (e.g., "26.1")

    [JsonProperty("reactionmeddrapt")]
    public string
        ReactionMeddraPt { get; set; } // MedDRA preferred term (e.g., "Intercepted product preparation error")

    [JsonProperty("reactionoutcome")]
    public string ReactionOutcome { get; set; } // Outcome: 1=Recovered, 2=Recovering, ..., 6=Unknown
}