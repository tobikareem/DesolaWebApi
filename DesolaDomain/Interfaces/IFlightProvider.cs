using amadeus.resources;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;

namespace DesolaDomain.Interfaces;

public interface IFlightProvider
{
    Task<UnifiedFlightSearchResponse> SearchFlightsAsync(FlightSearchParameters parameters,
        CancellationToken cancellationToken);

    string ProviderName { get; }
}