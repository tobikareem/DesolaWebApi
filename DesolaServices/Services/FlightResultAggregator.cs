using DesolaDomain.Entities.FlightSearch;

namespace DesolaServices.Services;

public static class FlightResultAggregator
{
    public static UnifiedFlightSearchResponse CombineResults(List<UnifiedFlightSearchResponse> responses, string sortBy, string sortOrder)
    {
        if (!responses.Any() || responses.All(r => r == null))
        {
            return new UnifiedFlightSearchResponse { Offers = new List<UnifiedFlightOffer>() };
        }

        var combinedAirlines = new Dictionary<string, string>();
        var combinedAirports = new Dictionary<string, string>();
        var combinedLocations = new Dictionary<string, AirportCity>();
        var airlineLogos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var allOffers = new List<UnifiedFlightOffer>();

        UnifiedFlightSearchResponse firstValidResponse = null;

        foreach (var response in responses.Where(response => response?.Offers != null))
        {

            if (firstValidResponse == null)
                firstValidResponse = response;

            allOffers.AddRange(response.Offers);

            foreach (var kv in response.Airlines ?? Enumerable.Empty<KeyValuePair<string, string>>())
                combinedAirlines[kv.Key] = kv.Value;

            foreach (var kv in response.Airports ?? Enumerable.Empty<KeyValuePair<string, string>>())
                combinedAirports[kv.Key] = kv.Value;

            foreach (var kv in response.Locations ?? Enumerable.Empty<KeyValuePair<string, AirportCity>>())
                combinedLocations[kv.Key] = kv.Value;

            if (response.Offers.All(x => x.FlightSource != "SkyScanner"))
                continue;

            foreach (var offer in response.Offers)
            {
                foreach (var itinerary in offer.Itineraries)
                {
                    foreach (var segment in itinerary.Segments)
                    {
                        if (!string.IsNullOrWhiteSpace(segment.AirlineLogo) && !string.IsNullOrWhiteSpace(segment.Id) && !airlineLogos.ContainsKey(segment.Id))
                        {
                            airlineLogos[segment.Id] = segment.AirlineLogo;
                        }
                    }
                }
            }
        }

        var sortedOffers = SortOffers(allOffers, sortBy, sortOrder);

        foreach (var offer in sortedOffers)
        {
            if (offer.FlightSource == "SkyScanner")
                continue;

            foreach (var itinerary in offer.Itineraries)
            {
                foreach (var segment in itinerary.Segments)
                {
                    if (string.IsNullOrWhiteSpace(segment.AirlineLogo) && !string.IsNullOrWhiteSpace(segment.MarketingAirline) && airlineLogos.TryGetValue(segment.MarketingAirline, out var logo))
                    {
                        segment.AirlineLogo = logo;
                    }
                }
            }
        }


        return new UnifiedFlightSearchResponse
        {
            Offers = sortedOffers,
            TotalResults = sortedOffers.Count,
            CurrencyCode = responses.FirstOrDefault()?.CurrencyCode ?? "USD",
            Airlines = combinedAirlines,
            Airports = combinedAirports,
            Locations = combinedLocations,
            Origin = responses.FirstOrDefault()?.Origin,
            Destination = responses.FirstOrDefault()?.Destination,
            DepartureDate = responses.FirstOrDefault()?.DepartureDate ?? DateTime.Now,
            ReturnDate = responses.FirstOrDefault()?.ReturnDate
        };
    }

    private static List<UnifiedFlightOffer> SortOffers(IEnumerable<UnifiedFlightOffer> offers, string sortBy, string sortOrder) {
        if (string.IsNullOrEmpty(sortBy))
        {
            return offers.OrderBy(o => o.TotalPrice).ToList();
        }

        return sortBy.ToLower() switch
        {
            "price" => sortOrder?.ToLower() == "desc"
                ? offers.OrderByDescending(o => o.TotalPrice).ToList()
                : offers.OrderBy(o => o.TotalPrice).ToList(),

            "duration" => sortOrder?.ToLower() == "desc"
                ? offers.OrderByDescending(o => o.Itineraries.FirstOrDefault()?.Duration.TotalMinutes ?? 0).ToList()
                : offers.OrderBy(o => o.Itineraries.FirstOrDefault()?.Duration.TotalMinutes ?? 0).ToList(),

            _ => offers.OrderBy(o => o.TotalPrice).ToList()
        };
    }
}