using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;

namespace DesolaServices.Interfaces;

public interface IFlightSearchService
{
    Task<FlightOffer> SearchFlightsAsync(FlightSearchBasic criteria);

    Task<amadeus.resources.FlightOffer[]> SearchAdvancedFlightsAsync(FlightSearchAdvanced criteria);
}