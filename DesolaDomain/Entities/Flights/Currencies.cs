using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Currencies
{
    [JsonPropertyName("EUR")]
    public string Eur { get; set; }
}