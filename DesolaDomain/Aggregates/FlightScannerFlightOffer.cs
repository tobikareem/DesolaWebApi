using System.Text.Json.Serialization;
using DesolaDomain.Entities.Flights;

namespace DesolaDomain.Aggregates;

public class SkyScannerFlightOffer
{
    [JsonPropertyName("data")]
    public SkyScannerData Data { get; set; }
}

public class SkyScannerData
{
    [JsonPropertyName("itineraries")]
    public List<SkyScannerItinerary> Itineraries { get; set; }
}

public class SkyScannerItinerary
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("price")]
    public SkyScannerPrice Price { get; set; }

    [JsonPropertyName("legs")]
    public List<SkyScannerLeg> Legs { get; set; }
}

public class SkyScannerPrice
{
    [JsonPropertyName("raw")]
    public decimal Raw { get; set; }

    [JsonPropertyName("formatted")]
    public string Formatted { get; set; }
}

public class SkyScannerLeg
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("origin")]
    public SkyScannerLocation Origin { get; set; }

    [JsonPropertyName("destination")]
    public SkyScannerLocation Destination { get; set; }

    [JsonPropertyName("durationInMinutes")]
    public int DurationInMinutes { get; set; }

    public int StopCount { get; set; }

    [JsonPropertyName("departure")]
    public DateTime Departure { get; set; }

    [JsonPropertyName("arrival")]
    public DateTime Arrival { get; set; }

    [JsonPropertyName("carriers")]
    public SkyScannerCarriers Carriers { get; set; }

    [JsonPropertyName("segments")]
    public List<SkySegment> Segments { get; set; }
}

public class SkyScannerLocation
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("displayCode")]
    public string DisplayCode { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("parent")]
    public Parent Parent { get; set; }
}

public class SkyScannerCarriers
{
    [JsonPropertyName("marketing")]
    public List<SkyScannerCarrier> Marketing { get; set; }
}

public class SkyScannerCarrier
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("logoUrl")]
    public string LogoUrl { get; set; }

    [JsonPropertyName("alternateId")]
    public string AlternateId { get; set; }
}

public class Parent
{
    [JsonPropertyName("flightPlaceId")]
    public string FlightPlaceId { get; set; }
    [JsonPropertyName("displayCode")]
    public string DisplayCode { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
}