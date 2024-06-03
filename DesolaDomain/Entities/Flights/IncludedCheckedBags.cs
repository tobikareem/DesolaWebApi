using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class IncludedCheckedBags
{
    [JsonPropertyName("weight")]
    public string Weight { get; set; }

    [JsonPropertyName("weightUnit")]
    public string WeightUnit { get; set; }
    
}