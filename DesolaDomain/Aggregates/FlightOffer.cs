using System.Text.Json.Serialization;
using DesolaDomain.Entities.Flights;

namespace DesolaDomain.Aggregates;


public class FlightOffer
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public List<Datum> Data { get; set; }

    [JsonPropertyName("dictionaries")]
    public Dictionaries Dictionaries { get; set; }
}