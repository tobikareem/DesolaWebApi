using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Datum
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("instantTicketingRequired")]
    public bool InstantTicketingRequired { get; set; }

    [JsonPropertyName("nonHomogeneous")]
    public bool NonHomogeneous { get; set; }

    [JsonPropertyName("oneWay")]
    public bool OneWay { get; set; }

    [JsonPropertyName("lastTicketingDate")]
    public DateTimeOffset LastTicketingDate { get; set; }

    [JsonPropertyName("lastTicketingDateTime")]
    public DateTimeOffset LastTicketingDateTime { get; set; }

    [JsonPropertyName("numberOfBookableSeats")]
    public long NumberOfBookableSeats { get; set; }

    [JsonPropertyName("itineraries")]
    public List<Itinerary> Itineraries { get; set; }

    [JsonPropertyName("price")]
    public DatumPrice Price { get; set; }

    [JsonPropertyName("pricingOptions")]
    public PricingOptions PricingOptions { get; set; }

    [JsonPropertyName("validatingAirlineCodes")]
    public List<string> ValidatingAirlineCodes { get; set; }

    [JsonPropertyName("travelerPricings")]
    public List<TravelerPricing> TravelerPricings { get; set; }
}