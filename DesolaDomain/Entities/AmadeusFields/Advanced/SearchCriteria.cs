namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class SearchCriteria
{
    public bool? ExcludeAllotments { get; set; }
    public bool? AddOneWayOffers { get; set; }
    public int? MaxFlightOffers { get; set; }
    public int? MaxPrice { get; set; }
    public bool? AllowAlternativeFareOptions { get; set; }
    public bool? OneFlightOfferPerDay { get; set; }
    public PricingOptions PricingOptions { get; set; }
    public FlightFilters FlightFilters { get; set; }
}