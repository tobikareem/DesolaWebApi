using DesolaDomain.Model;

namespace DesolaDomain.Interfaces;

public interface IAirportRepository
{
    Task<IEnumerable<Airport>> SearchAirportsAsync(string query);
    Task<List<Airport>> GetAirportsAsync();
    Task<bool> IsAirportValidAsync(string airportCode);
}