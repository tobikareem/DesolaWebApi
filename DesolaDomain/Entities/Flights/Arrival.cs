using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Arrival
{
    [JsonPropertyName("iataCode")]
    public string IataCode { get; set; }

    [JsonPropertyName("at")]
    public DateTimeOffset At { get; set; }
}