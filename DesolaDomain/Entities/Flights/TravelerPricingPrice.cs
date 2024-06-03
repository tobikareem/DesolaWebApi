using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class TravelerPricingPrice
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("total")]
    public string Total { get; set; }

    [JsonPropertyName("base")]
    public string Base { get; set; }
}