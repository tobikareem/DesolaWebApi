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

public class OriginDestination
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("originLocationCode")]
    public string OriginLocationCode { get; set; }

    [JsonPropertyName("destinationLocationCode")]
    public string DestinationLocationCode { get; set; }

    [JsonPropertyName("departureDateTimeRange")]
    public DepartureDateTimeRange DepartureDateTimeRange { get; set; }
}

public class DepartureDateTimeRange
{
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }
}

public class SearchCriteria
{
    [JsonPropertyName("maxFlightOffers")]
    public long MaxFlightOffers { get; set; }

    [JsonPropertyName("flightFilters")]
    public FlightFilters FlightFilters { get; set; }
}

public class FlightFilters
{
    [JsonPropertyName("cabinRestrictions")]
    public List<CabinRestriction> CabinRestrictions { get; set; }

    [JsonPropertyName("carrierRestrictions")]
    public CarrierRestrictions CarrierRestrictions { get; set; }
}

public class CabinRestriction
{
    [JsonPropertyName("cabin")]
    public string Cabin { get; set; }

    [JsonPropertyName("coverage")]
    public string Coverage { get; set; }

    [JsonPropertyName("originDestinationIds")]
    public List<string> OriginDestinationIds { get; set; }
}

public class CarrierRestrictions
{
    [JsonPropertyName("excludedCarrierCodes")]
    public List<string> ExcludedCarrierCodes { get; set; }
}

public class Traveler
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("travelerType")]
    public string TravelerType { get; set; }

    [JsonPropertyName("fareOptions")]
    public List<string> FareOptions { get; set; }
}