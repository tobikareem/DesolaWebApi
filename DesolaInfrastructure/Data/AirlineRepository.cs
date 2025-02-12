using System.Text.Json;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DesolaInfrastructure.Data;

public class AirlineRepository : IAirlineRepository
{
    private readonly IBlobStorageRepository _blobStorageRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AirlineRepository> _logger;

    private readonly string _fileName;
    private readonly string _containerName;
    private readonly string _americanAirlines;

    public AirlineRepository(IBlobStorageRepository blobStorageRepository, ICacheService cacheService, ILogger<AirlineRepository> logger, IConfiguration configuration)
    {
        _blobStorageRepository = blobStorageRepository;
        _cacheService = cacheService;
        _logger = logger;

        _fileName = configuration["Airport_Code_File"] ?? throw new ArgumentNullException(nameof(configuration), "Unable to find airline file name");
        _containerName = configuration["ContainerName"] ?? throw new ArgumentNullException(nameof(configuration), "Unable to find airline container name");
        _americanAirlines = configuration["AmericanAirlines"] ?? throw new ArgumentNullException(nameof(configuration), "Unable to find american airlines");
    }

    public async Task<List<Airline>>  GetAllAirlinesAsync()
    {
        _logger.LogInformation("Getting all airlines");

        var airlines = _cacheService.GetItem<List<Airline>>(CacheEntry.AllAirlines);

        if (airlines != null)
        {
            return airlines;
        }


        if (!await _blobStorageRepository.DoesBlobExistAsync(_fileName, _containerName))
        {
            throw new InvalidOperationException("Airline file not found");
        }

        var airlineFile = await _blobStorageRepository.DownloadBlobAsStringAsync(_fileName, _containerName);

        var airlineList = JsonSerializer.Deserialize<List<Airline>>(airlineFile);

        _cacheService.Add(CacheEntry.AllAirlines, airlineList, TimeSpan.FromDays(30));

        return airlineList ?? throw new ArgumentNullException(nameof(Airline), "Airline list is null");

    }

    public async Task<List<Airline>> GetAmericanAirlinesAsync()
    {

        var allAirLines = await GetAllAirlinesAsync();

        return allAirLines.Where(x => _americanAirlines.Contains(x.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<string> GetAllAirlineIataCodesAsync()
    {
        var allAirLines = await GetAllAirlinesAsync();

        var codes = allAirLines.Select(x => x.IataCode);

        return string.Join(",", codes);
    }

    public async Task<string> GetAllAmericanAirlineIataCodesAsync()
    {
        var allAirLines = await GetAmericanAirlinesAsync();

        var codes = allAirLines.Select(x => x.IataCode);

        return string.Join(",", codes);
    }
}