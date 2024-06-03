using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Location
{
    [JsonPropertyName("cityCode")]
    public string CityCode { get; set; }

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; }
}