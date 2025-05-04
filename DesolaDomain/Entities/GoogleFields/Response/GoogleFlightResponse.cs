using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.GoogleFields.Response;

public class GoogleFlightResponse
{
    [JsonPropertyName("data")]
    public FlightData Data { get; set; }

    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class FlightData
{
    [JsonPropertyName("topFlights")]
    public List<Flight> TopFlights { get; set; }

    [JsonPropertyName("otherFlights")]
    public List<Flight> OtherFlights { get; set; }

    [JsonPropertyName("filters")]
    public Filters Filters { get; set; }

    [JsonPropertyName("priceHistory")]
    public List<PriceHistoryPoint> PriceHistory { get; set; }
}

public class Flight
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("detailToken")]
    public string DetailToken { get; set; }

    [JsonPropertyName("airlineCode")]
    public string AirlineCode { get; set; }

    [JsonPropertyName("airlineNames")]
    public List<string> AirlineNames { get; set; }

    [JsonPropertyName("segments")]
    public List<FlightSegment> Segments { get; set; }

    [JsonPropertyName("departureAirportCode")]
    public string DepartureAirportCode { get; set; }

    [JsonPropertyName("departureDate")]
    public string DepartureDate { get; set; }

    [JsonPropertyName("departureTime")]
    public string DepartureTime { get; set; }

    [JsonPropertyName("arrivalAirportCode")]
    public string ArrivalAirportCode { get; set; }

    [JsonPropertyName("arrivalDate")]
    public string ArrivalDate { get; set; }

    [JsonPropertyName("arrivalTime")]
    public string ArrivalTime { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("stops")]
    public int Stops { get; set; }

    [JsonPropertyName("isCodeShare")]
    public bool IsCodeShare { get; set; }

    [JsonPropertyName("hasTransit")]
    public bool? HasTransit { get; set; }

    [JsonPropertyName("transferAirports")]
    public object TransferAirports { get; set; }

    [JsonPropertyName("fareId")]
    public object FareId { get; set; }

    [JsonPropertyName("metaData")]
    public object MetaData { get; set; }

    [JsonPropertyName("baggage")]
    public object Baggage { get; set; }

    [JsonPropertyName("airline")]
    public List<AirlineInfo> Airline { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }
}

public class FlightSegment
{
    [JsonPropertyName("departureAirportCode")]
    public string DepartureAirportCode { get; set; }

    [JsonPropertyName("departureAirportName")]
    public string DepartureAirportName { get; set; }

    [JsonPropertyName("arrivalAirportName")]
    public string ArrivalAirportName { get; set; }

    [JsonPropertyName("arrivalAirportCode")]
    public string ArrivalAirportCode { get; set; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; set; }

    [JsonPropertyName("departureTime")]
    public string DepartureTime { get; set; }

    [JsonPropertyName("arrivalTime")]
    public string ArrivalTime { get; set; }

    [JsonPropertyName("cabinClass")]
    public int CabinClass { get; set; }

    [JsonPropertyName("seatPitch")]
    public string SeatPitch { get; set; }

    [JsonPropertyName("airlineIndex")]
    public List<CodeShareAirline> AirlineIndex { get; set; }

    [JsonPropertyName("aircraftType")]
    public string AircraftType { get; set; }

    [JsonPropertyName("aircraftName")]
    public string AircraftName { get; set; }

    [JsonPropertyName("overnight")]
    public bool Overnight { get; set; }

    [JsonPropertyName("delayed")]
    public object Delayed { get; set; }

    [JsonPropertyName("departureDate")]
    public string DepartureDate { get; set; }

    [JsonPropertyName("arrivalDate")]
    public string ArrivalDate { get; set; }

    [JsonPropertyName("airline")]
    public SegmentAirline Airline { get; set; }

    [JsonPropertyName("seatWidth")]
    public string SeatWidth { get; set; }

    [JsonPropertyName("flightId")]
    public int FlightId { get; set; }

    [JsonPropertyName("someFlag")]
    public int SomeFlag { get; set; }
}

public class SegmentAirline
{
    [JsonPropertyName("airlineCode")]
    public string AirlineCode { get; set; }

    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; }

    [JsonPropertyName("airlineName")]
    public string AirlineName { get; set; }
}

public class CodeShareAirline
{
    [JsonPropertyName("airlineCode")]
    public string AirlineCode { get; set; }

    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; }

    [JsonPropertyName("airlineName")]
    public string AirlineName { get; set; }
}

public class AirlineInfo
{
    [JsonPropertyName("airlineCode")]
    public string AirlineCode { get; set; }

    [JsonPropertyName("airlineName")]
    public string AirlineName { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }
}

public class Filters
{
    [JsonPropertyName("alliances")]
    public List<Alliance> Alliances { get; set; }

    [JsonPropertyName("airlines")]
    public List<AirlineFilter> Airlines { get; set; }

    [JsonPropertyName("airports")]
    public List<AirportFilter> Airports { get; set; }
}

public class Alliance
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class AirlineFilter
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class AirportFilter
{
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class PriceHistoryPoint
{
    [JsonPropertyName("time")]
    public long Time { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
