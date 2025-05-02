using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using MediatR;

namespace DesolaServices.Commands.Queries.FlightSearch;

public class GetBasicFlightSearchQuery: IRequest<ValueTuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>>
{
    public FlightSearchParameters SearchParameters { get; }
    public GetBasicFlightSearchQuery(FlightSearchParameters searchParameters)
    {
        SearchParameters = searchParameters;
    }
}