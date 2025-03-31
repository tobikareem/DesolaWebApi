using System.Text.Json.Serialization;
using DesolaDomain.Entities.Flights;

namespace DesolaDomain.Aggregates;


public class FlightOffer
{
    public Guid Id { get; private set; }

    [JsonPropertyName("meta")]
    public Meta Meta { get; private set; }

    [JsonPropertyName("data")]
    public List<Datum> Data { get; private set; }

    [JsonPropertyName("dictionaries")]
    public Dictionaries Dictionaries { get; private set; }

    public static FlightOffer FromApiResponse(Meta meta, List<Datum> data, Dictionaries dictionaries)
    {
        return new FlightOffer
        {
            Id = Guid.NewGuid(),
            Meta = meta,
            Data = data,
            Dictionaries = dictionaries
        };
    }

    public decimal CalculateTotalPrice()
    {
        return Data?.Sum(d => int.Parse(d.Price.GrandTotal)) ?? 0;
    }

    public bool IsHiddenCityOpportunity(string desiredDestination)
    {
        return Data?.Any(d => d.Itineraries?.Any(i =>
            i.Segments?.Any(s => s.Arrival?.IataCode == desiredDestination &&
                                 s.Arrival?.IataCode != d.Itineraries.Last().Segments.Last().Arrival?.IataCode)
            ?? false) ?? false) ?? false;
    }
}