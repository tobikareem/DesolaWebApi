using amadeus;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using Microsoft.Extensions.Logging;
using FlightOffer = amadeus.resources.FlightOffer;

namespace DesolaInfrastructure.External;

public class AmadeusService:IAmadeusService
{
    private readonly ILogger<AmadeusService> _logger;

    private readonly Amadeus _amadeus;
    public AmadeusService(Amadeus amadeus, ILogger<AmadeusService> logger)
    {
        _amadeus = amadeus;
        _logger = logger;
    }
    public FlightOffer[] PostFlightOffers(string body)
    {
        throw new System.NotImplementedException(body);
    }

    public FlightOffer[] GetFlightOffers(string origin, string destination, string departureDate, string returnDate, string adults,
        int children, int infants, string travelClass, string currencyCode)
    {
        _logger.LogInformation("Getting flight offers");

        var flightOffers = _amadeus.shopping.flightOffers.get(Params.with(FlightSearchParameter.OriginLocationCode, origin)
            .and(FlightSearchParameter.DestinationLocationCode, destination)
            .and(FlightSearchParameter.DepartureDate, departureDate)
            .and(FlightSearchParameter.ReturnDate, returnDate)
            .and(FlightSearchParameter.Adults, adults)
            .and(FlightSearchParameter.Max, "5"));

        return flightOffers;
    }
}