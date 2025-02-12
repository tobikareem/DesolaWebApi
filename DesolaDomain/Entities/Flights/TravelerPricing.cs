using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class TravelerPricing
{
    [JsonPropertyName("travelerId")]
    public string TravelerId { get; set; }

    [JsonPropertyName("fareOption")]
    public string FareOption { get; set; }

    [JsonPropertyName("travelerType")]
    public string TravelerType { get; set; }

    [JsonPropertyName("price")]
    public TravelerPricingPrice Price { get; set; }

    [JsonPropertyName("fareDetailsBySegment")]
    public List<FareDetailsBySegment> FareDetailsBySegment { get; set; }
}