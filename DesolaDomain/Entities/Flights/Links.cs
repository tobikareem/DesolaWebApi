using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Links
{
    [JsonPropertyName("self")]
    public Uri Self { get; set; }
}