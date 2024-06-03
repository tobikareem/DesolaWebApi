using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Itinerary
{
    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("segments")]
    public List<Segment> Segments { get; set; }
}