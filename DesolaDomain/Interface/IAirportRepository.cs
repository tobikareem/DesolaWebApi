using DesolaDomain.Model;

namespace DesolaDomain.Interface;

public interface IAirportRepository
{
    public Task<List<Airport>> GetAirportsAsync();
}