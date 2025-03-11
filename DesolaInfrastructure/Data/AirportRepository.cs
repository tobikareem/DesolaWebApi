using System.Globalization;
using CaptainOath.DataStore.Interface;
using CsvHelper;
using CsvHelper.Configuration;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;

namespace DesolaInfrastructure.Data;

public class AirportRepository : IAirportRepository
{
    private readonly IBlobClientRepository _blobStorageRepository;
    private readonly ICacheService _cacheService;
    public AirportRepository(IBlobClientRepository blobStorageRepository, ICacheService cacheService)
    {
        _blobStorageRepository = blobStorageRepository;
        _cacheService=cacheService;
    }
    public async Task<List<Airport>> GetAirportsAsync()
    {
        return await Task.Run(GetAllAirports);
    }

    public async Task<bool> IsAirportValidAsync(string airportCode)
    {
        var airports = await GetAllAirports();
        return airports.Any(airport => airport.Code == airportCode);
    }

    private async Task<List<Airport>> GetAllAirports()
    {
        if (_cacheService.Contains(CacheEntry.AllAirports))
        {
            return _cacheService.GetItem<List<Airport>>(CacheEntry.AllAirports) ?? new List<Airport>();
        }

        if (!await _blobStorageRepository.DoesBlobExistAsync())
        {
            return new List<Airport>();
        }

        var stream = await _blobStorageRepository.DownloadBlobAsStreamAsync();

        var airports = GetAirportsAsync(stream);
        _cacheService.Add(CacheEntry.AllAirports, airports, TimeSpan.FromDays(30));

        return airports;
    }

    private static List<Airport> GetAirportsAsync(Stream stream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
        };
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<AirportCsvMap>();

        var records = csv.GetRecords<Airport>()
            .Where(record => !string.IsNullOrWhiteSpace(record.Code))
            .ToList();
        return records;
    }
    
}