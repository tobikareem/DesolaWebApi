using System.Text.Json.Serialization;

namespace DesolaServices.DataTransferObjects.Requests;


public class FlightSearchAdvancedRequest
{
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; }

    [JsonPropertyName("originDestinations")]
    public List<OriginDestination> OriginDestinations { get; set; }

    [JsonPropertyName("travelers")]
    public List<Traveler> Travelers { get; set; }

    [JsonPropertyName("sources")]
    public List<string> Sources { get; set; }

    [JsonPropertyName("searchCriteria")]
    public SearchCriteria SearchCriteria { get; set; }

    public string SortBy { get; set; }
    public string SortOrder { get; set; }

    public int MaxNumberOfStopOver { get; set; }
}