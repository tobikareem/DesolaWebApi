using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;

public class SearchCriteria
{
    [JsonPropertyName("maxFlightOffers")]
    public long MaxFlightOffers { get; set; }

    [JsonPropertyName("flightFilters")]
    public FlightFilters FlightFilters { get; set; }
}