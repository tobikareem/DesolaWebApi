using amadeus.resources;

namespace DesolaDomain.Interfaces;

public interface IAmadeusService
{
    FlightOffer [] PostFlightOffers(string body);

    FlightOffer[] GetFlightOffers(string origin, string destination, string departureDate, string returnDate,
        string adults, int children, int infants, string travelClass, string currencyCode);
}