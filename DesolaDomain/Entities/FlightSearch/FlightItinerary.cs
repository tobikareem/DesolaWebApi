namespace DesolaDomain.Entities.FlightSearch;

public class FlightItinerary
{
    // Unique identifier for this itinerary
    public string ItineraryId { get; set; }

    // Pricing information
    public PriceDetails Price { get; set; }

    // Total duration in minutes
    public int TotalDurationMinutes { get; set; }

    // Formatted duration (e.g., "5h 30m")
    public string FormattedDuration { get; set; }

    // Flight segments that make up this itinerary
    public List<FlightSegment> Segments { get; set; }

    // Data source information
    public SourceInfo Source { get; set; }

    // Baggage allowance for the entire itinerary
    public BaggageAllowance Baggage { get; set; }

    // Booking link or deep link
    public string BookingLink { get; set; }

    // Availability information
    public int SeatsAvailable { get; set; }

    // Special classifications or tags
    public List<string> Tags { get; set; }

    // Value scores and rankings
    public ValueScores Scores { get; set; }

    // Fare rules and restrictions
    public FareRules FareRules { get; set; }

    // Special deals or opportunities detected
    public SpecialOpportunity SpecialOpportunity { get; set; }
}