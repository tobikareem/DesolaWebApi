using DesolaDomain.Model;

namespace DesolaDomain.Interfaces;

public interface IAirportRepository
{
    public Task<List<Airport>> GetAirportsAsync();
}