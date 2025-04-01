using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerCarriers
{
    [JsonPropertyName("marketing")]
    public List<SkyScannerCarrier> Marketing { get; set; }
}