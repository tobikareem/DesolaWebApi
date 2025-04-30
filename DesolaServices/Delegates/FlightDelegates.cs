using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;

namespace DesolaServices.Delegates;

public delegate Task<UnifiedFlightSearchResponse> FlightProviderDelegate(FlightSearchParameters parameters, CancellationToken cancellationToken);
