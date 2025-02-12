using System.Text.Json.Serialization;

namespace DesolaDomain.Model;

public class Airline
{
    [JsonPropertyName("iata_code")]
    public string IataCode { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("icao_code")]
    public string IcaoCode { get; set; }
    
}