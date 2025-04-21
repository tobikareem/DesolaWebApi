namespace DesolaDomain.Entities.FlightSearch;

public class UnifiedFlightSearchResponse
{
    public UnifiedFlightSearchResponse()
    {
        Metadata = new SearchMetadata();
    }
    public int TotalResults { get; set; }
    public string CurrencyCode { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public IEnumerable<UnifiedFlightOffer> Offers { get; set; }
    public Dictionary<string, string> Airlines { get; set; } = new();
    public Dictionary<string, string> Airports { get; set; } = new();
    public Dictionary<string, AirportCity> Locations { get; set; } = new();
    public SearchMetadata Metadata { get; set; }
}

public class UnifiedFlightOffer
{
    public string Id { get; set; }
    public string Provider { get; set; } // "Amadeus", "SkyScanner", etc.
    public string FlightSource { get; set; }
    public decimal TotalPrice { get; set; }
    public string FormattedPrice { get; set; } // "$332.20"
    public IEnumerable<UnifiedItinerary> Itineraries { get; set; }
    public BaggageAllowance BaggageAllowance { get; set; }
    public bool IsRefundable { get; set; }
    public DateTime? LastTicketingDate { get; set; }
    public string ValidatingCarrier { get; set; }
    public int AvailableSeats { get; set; }
    public List<string> FareConditions { get; set; } = new();
}

public class UnifiedItinerary
{
    public string Direction { get; set; } // "Outbound" or "Return"
    public TimeSpan Duration { get; set; }
    public string FormattedDuration { get; set; } // "9h 20m"
    public int Stops { get; set; }
    public IEnumerable<UnifiedSegment> Segments { get; set; }
}

public class UnifiedSegment
{
    public string Id { get; set; }
    public UnifiedLocation Departure { get; set; }
    public UnifiedLocation Arrival { get; set; }
    public TimeSpan Duration { get; set; }
    public string FormattedDuration { get; set; } // "9h 20m"
    public string MarketingAirline { get; set; }
    public string OperatingAirline { get; set; }
    public string FlightNumber { get; set; }
    public string AircraftType { get; set; }
    public string CabinClass { get; set; }
    public BaggageAllowance BaggageAllowance { get; set; }
}

public class UnifiedLocation
{
    public string AirportCode { get; set; }
    public string Terminal { get; set; }
    public string CityCode { get; set; }
    public string CountryCode { get; set; }
    public DateTime DateTime { get; set; }
    public string FormattedDateTime { get; set; } // "May 2, 10:00 AM"
}

public class BaggageAllowance
{
    public int CheckedBags { get; set; }
    public int? WeightKg { get; set; }
    public string Description { get; set; }
}

public class AirportCity
{
    public string CityCode { get; set; }
    public string CountryCode { get; set; }
    public string CityName { get; set; }
    public string CountryName { get; set; }
}