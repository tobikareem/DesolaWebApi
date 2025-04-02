using System.Text.Json.Serialization;
using amadeus.resources;
using Newtonsoft.Json;

namespace DesolaDomain.Entities.AmadeusFields.Response;

public class AmadeusFlightOffersResponse
{
    [JsonProperty("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public List<FlightOffer> Data { get; set; }

}