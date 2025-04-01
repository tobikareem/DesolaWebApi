

namespace DesolaDomain.Entities.FlightSearch;

public class FlightSearchResponse
{
    // Top-level response structure
    public List<FlightItinerary> Itineraries { get; set; }
    public SearchStats Stats { get; set; }
    public SearchMetadata Meta { get; set; }
}