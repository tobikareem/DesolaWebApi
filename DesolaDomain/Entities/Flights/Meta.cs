using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Meta
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("links")]
    public Links Links { get; set; }
}