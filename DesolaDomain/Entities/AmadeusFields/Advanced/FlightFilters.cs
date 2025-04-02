namespace DesolaDomain.Entities.AmadeusFields.Advanced;

public class FlightFilters
{
    public bool? CrossBorderAllowed { get; set; }
    public bool? MoreOvernightsAllowed { get; set; }
    public bool? ReturnToDepartureAirport { get; set; }
    public bool? RailSegmentAllowed { get; set; }
    public bool? BusSegmentAllowed { get; set; }
    public double? MaxFlightTime { get; set; }
    public List<string> IncludedCarrierCodes { get; set; }
    public List<string> ExcludedCarrierCodes { get; set; }
    public List<CabinRestriction> CabinRestrictions { get; set; }
    public ConnectionRestriction ConnectionRestriction { get; set; }
}