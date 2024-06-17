using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Interfaces;

public interface IFlightSearchService
{
    Task<Dictionary<string, FlightItineraryGroupResponse>> SearchFlightsAsync(FlightSearchBasicRequest criteria, CancellationToken cancellationToken);

    Task<Dictionary<string, FlightItineraryGroupResponse>> SearchAdvancedFlightsAsync(
        FlightSearchAdvancedRequest criteria, CancellationToken cancellationToken);
}