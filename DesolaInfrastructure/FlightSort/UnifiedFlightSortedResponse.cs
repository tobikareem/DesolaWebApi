using DesolaDomain.Entities.FlightSearch;

namespace DesolaInfrastructure.FlightSort;

public class UnifiedFlightSortedResponse
{
    public static void ApplySorting(UnifiedFlightSearchResponse flightSearchResponse, string sortBy, string sortOrder)
    {
        if(flightSearchResponse == null || !flightSearchResponse.Offers.Any())
        {
            return;
        }

        sortBy = string.IsNullOrEmpty(sortBy) ? "price" : sortBy;

        var isAscending = string.IsNullOrWhiteSpace(sortOrder) || string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase);

        flightSearchResponse.Offers = sortBy.ToLower() switch
        {
            "price" => isAscending
                ? flightSearchResponse.Offers.OrderBy(x => x.TotalPrice)
                : flightSearchResponse.Offers.OrderByDescending(x => x.TotalPrice),
            "duration" => isAscending
                ? flightSearchResponse.Offers.OrderBy(o => o.Itineraries.First().Duration.TotalMinutes)
                : flightSearchResponse.Offers.OrderByDescending(o => o.Itineraries.First().Duration.TotalMinutes),
            "departure" => isAscending
                ? flightSearchResponse.Offers.OrderBy(GetDepartureTime)
                : flightSearchResponse.Offers.OrderByDescending(GetDepartureTime),
            "arrival" => isAscending
                ? flightSearchResponse.Offers.OrderBy(GetArrivalTime)
                : flightSearchResponse.Offers.OrderByDescending(GetArrivalTime),
            "stops" => isAscending
                ? flightSearchResponse.Offers.OrderBy(o => o.Itineraries.First().Stops)
                : flightSearchResponse.Offers.OrderByDescending(o => o.Itineraries.First().Stops),
            "airline" => isAscending
                ? flightSearchResponse.Offers.OrderBy(GetMainAirline)
                : flightSearchResponse.Offers.OrderByDescending(GetMainAirline),
            "overall" => isAscending
                ? flightSearchResponse.Offers.OrderBy(CalculateOverallScore)
                : flightSearchResponse.Offers.OrderByDescending(CalculateOverallScore),
            _ => flightSearchResponse.Offers.OrderBy(o => o.TotalPrice)
        };
    }

    private static DateTime GetDepartureTime(UnifiedFlightOffer offer)
    {
        return offer.Itineraries.FirstOrDefault()?.Segments.FirstOrDefault()?.Departure?.DateTime ?? DateTime.MinValue;
    }

    private static DateTime GetArrivalTime(UnifiedFlightOffer offer)
    {
        var itinerary = offer.Itineraries.FirstOrDefault();
        if (itinerary == null || !itinerary.Segments.Any())
            return DateTime.MinValue;

        return itinerary.Segments.Last().Arrival?.DateTime ?? DateTime.MinValue;
    }

    private static string GetMainAirline(UnifiedFlightOffer offer)
    {
        return offer.Itineraries.FirstOrDefault()?.Segments.FirstOrDefault()?.MarketingAirline ?? string.Empty;
    }

    private static double CalculateOverallScore(UnifiedFlightOffer offer)
    {
        if (!offer.Itineraries.Any())
            return double.MaxValue;

        // Weight factors (adjust as needed)
        const double priceWeight = 0.5;
        const double durationWeight = 0.3;
        const double stopsWeight = 0.2;

        // Normalize price (lower is better)
        var normalizedPrice = offer.TotalPrice / (decimal)1000.0; // Assuming most prices are under $1000

        // Normalize duration (lower is better)
        var duration = offer.Itineraries.First().Duration.TotalHours;
        var normalizedDuration = duration / 24.0; // Normalize against a 24-hour baseline
        
        var stops = offer.Itineraries.First().Stops;
        var normalizedStops = stops / 3.0; // Normalize against a 3-stop baseline
        
        return (double)((normalizedPrice * (decimal)priceWeight) +
                        (decimal)(normalizedDuration * durationWeight) +
                        (decimal)(normalizedStops * stopsWeight));
    }
}