using DesolaDomain.Model;

namespace DesolaDomain.Interface;

public interface IAirportRepository
{
    public Task<IEnumerable<Airport>> GetAirportsAsync();
}