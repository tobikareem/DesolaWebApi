using DesolaDomain.Model;

namespace DesolaDomain.Interfaces;

public interface IAirportRepository
{
    public Task<List<Airport>> GetAirportsAsync();
    Task<bool> IsAirportValidAsync(string airportCode);
}