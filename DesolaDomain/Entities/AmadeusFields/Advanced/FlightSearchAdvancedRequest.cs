namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class FlightSearchAdvancedRequest
{
    public string CurrencyCode { get; set; } = "USD";
    public List<OriginDestination> OriginDestinations { get; set; }
    public List<Traveler> Travelers { get; set; }
    public List<string> Sources { get; set; } = new List<string> { "GDS" };
    public SearchCriteria SearchCriteria { get; set; }
    public string SortBy { get; set; }
    public string SortOrder { get; set; }
}