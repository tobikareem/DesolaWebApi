using DesolaDomain.Model;

namespace DesolaDomain.Interfaces;

public interface IAirlineRepository
{
    Task<List<Airline>> GetAllAsync();

  Task<Airline> GetByCodeAsync(string iataCode);
}