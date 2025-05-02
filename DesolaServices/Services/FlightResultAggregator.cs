using DesolaDomain.Entities.FlightSearch;

namespace DesolaServices.Services;

public static class FlightResultAggregator
{
    public static UnifiedFlightSearchResponse CombineResults(List<UnifiedFlightSearchResponse> responses, string sortBy, string sortOrder)
    {
        if (!responses.Any())
        {
            return new UnifiedFlightSearchResponse { Offers = new List<UnifiedFlightOffer>() };
        }
        
        var combinedAirlines = new Dictionary<string, string>();
        var combinedAirports = new Dictionary<string, string>();
        var combinedLocations = new Dictionary<string, AirportCity>();
        
        var allOffers = new List<UnifiedFlightOffer>();

        foreach (var response in responses.Where(response => response?.Offers != null))
        {
            allOffers.AddRange(response.Offers);
            
            foreach (var airline in response.Airlines)
            {
                combinedAirlines[airline.Key] = airline.Value;
            }

            foreach (var airport in response.Airports)
            {
                combinedAirports[airport.Key] = airport.Value;
            }

            foreach (var location in response.Locations)
            {
                combinedLocations[location.Key] = location.Value;
            }
        }
        
        var sortedOffers = SortOffers(allOffers, sortBy, sortOrder);

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