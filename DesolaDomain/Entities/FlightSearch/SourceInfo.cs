namespace DesolaDomain.Entities.FlightSearch;

public class SourceInfo
{
    public string Provider { get; set; } // e.g., "Amadeus", "Kiwi", "SkyScanner"
    public string BookingAgent { get; set; } // e.g., "Expedia", "Airline Direct"
    public string FareType { get; set; } // e.g., "Published", "Private", "Corporate"
    public bool IsNDCSource { get; set; } // Whether this came from NDC API
    public string DataFreshness { get; set; } // e.g., "Live", "Cached (5m)"
}