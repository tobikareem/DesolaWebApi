using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Aircraft
{
    [JsonPropertyName("code")]
    public string Code { get; set; }
}