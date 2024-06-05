using DesolaDomain.Model;

namespace DesolaDomain.Interfaces;

public interface IAirlineRepository
{
    Task<List<Airline>> GetAllAirlinesAsync();

    Task<List<Airline>> GetAmericanAirlinesAsync();

    Task<string> GetAllAirlineIataCodesAsync();
}