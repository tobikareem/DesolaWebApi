using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights
{
    public class Address
    {
        [JsonPropertyName("countryName")]
        public string CountryName;

        [JsonPropertyName("countryCode")]
        public string CountryCode;

        [JsonPropertyName("stateCode")]
        public string StateCode;

        [JsonPropertyName("regionCode")]
        public string RegionCode;
    }
}
