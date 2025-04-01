namespace DesolaDomain.Entities.FlightSearch;

public class FlightSegment
{
    // Unique identifier for this segment
    public string SegmentId { get; set; }

    // Departure information
    public string DepartureAirport { get; set; }
    public string DepartureCity { get; set; }
    public string DepartureTerminal { get; set; }
    public DateTime DepartureTime { get; set; }
    public string DepartureTimeFormatted { get; set; } // e.g., "Mon, 12 Apr 2023 08:30"

    // Arrival information
    public string ArrivalAirport { get; set; }
    public string ArrivalCity { get; set; }
    public string ArrivalTerminal { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string ArrivalTimeFormatted { get; set; }

    // Duration
    public int DurationMinutes { get; set; }
    public string FormattedDuration { get; set; }

    // Airline information
    public string MarketingCarrier { get; set; } // Airline selling the flight
    public string MarketingCarrierName { get; set; }
    public string OperatingCarrier { get; set; } // Airline operating the flight
    public string OperatingCarrierName { get; set; }
    public string FlightNumber { get; set; }

    // Aircraft details
    public string AircraftType { get; set; }
    public string AircraftDescription { get; set; }

    // Cabin and seat information
    public string CabinClass { get; set; } // e.g., "economy", "business"
    public string BookingClass { get; set; } // Fare class code
    public SeatDetails Seats { get; set; }

    // Layover information (if this is part of a multi-segment journey)
    public int? LayoverMinutes { get; set; }
    public string LayoverAirport { get; set; }

    // Segment-specific baggage rules (may differ from itinerary level)
    public BaggageAllowance SegmentBaggage { get; set; }
}