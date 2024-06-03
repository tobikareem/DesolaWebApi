using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class PricingOptions
{
    [JsonPropertyName("fareType")]
    public List<string> FareType { get; set; }

    [JsonPropertyName("includedCheckedBagsOnly")]
    public bool IncludedCheckedBagsOnly { get; set; }
}