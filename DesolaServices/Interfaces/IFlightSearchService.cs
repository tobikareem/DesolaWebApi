using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Interfaces;

public interface IFlightSearchService
{
    Task<Dictionary<string, FlightItineraryGroupResponse>> SearchFlightsAsync(FlightSearchBasicRequest criteria);

    Task<Dictionary<string, FlightItineraryGroupResponse>> SearchAdvancedFlightsAsync(
        FlightSearchAdvancedRequest criteria);
}