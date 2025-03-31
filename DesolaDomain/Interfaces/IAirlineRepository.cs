using DesolaDomain.Model;

namespace DesolaDomain.Interfaces;

public interface IAirlineRepository
{
    Task<List<Airline>> GetAllAsync();

    Task<IEnumerable<Airline>> GetByCountryAsync(string countryCode);

    Task<Airline> GetByCodeAsync(string iataCode);
}