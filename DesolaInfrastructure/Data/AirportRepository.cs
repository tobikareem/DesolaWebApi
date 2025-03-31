using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using CaptainOath.DataStore.Interface;

namespace DesolaInfrastructure.Data;

public class AirportRepository : IAirportRepository
{
    private readonly IBlobClientRepository _blobStorageRepository;
    private readonly ICacheService _cacheService;
    private static readonly HashSet<string> ExcludedAirportTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "heliport", "closed", "seaplane", "seaplane_base"
    };

    public AirportRepository(IBlobClientRepository blobStorageRepository, ICacheService cacheService)
    {
        _blobStorageRepository = blobStorageRepository;
        _cacheService = cacheService;
    }

    public Task<List<Airport>> GetAirportsAsync()
    {
        return GetAllAirportsAsync();
    }

    public async Task<bool> IsAirportValidAsync(string airportCode)
    {
        if (_cacheService.Contains(CacheEntry.AllAirports))
        {
            var airports = _cacheService.GetItem<List<Airport>>(CacheEntry.AllAirports);
            return airports?.Any(airport => string.Equals(airport.Code, airportCode, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        await foreach (var airport in ReadUsAirportsAsync())
        {
            if (string.Equals(airport.Code, airportCode, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private async Task<List<Airport>> GetAllAirportsAsync()
    {
        if (_cacheService.Contains(CacheEntry.AllAirports))
        {
            return _cacheService.GetItem<List<Airport>>(CacheEntry.AllAirports) ?? new List<Airport>();
        }

        var airports = new List<Airport>();
        await foreach (var airport in ReadUsAirportsAsync())
        {
            airports.Add(airport);
        }

        if (airports.Count > 0)
        {
            _cacheService.Add(CacheEntry.AllAirports, airports, TimeSpan.FromDays(30));
        }

        return airports;
    }

    private async IAsyncEnumerable<Airport> ReadUsAirportsAsync()
    {
        if (!await _blobStorageRepository.DoesBlobExistAsync())
            yield break;

        using var stream = await _blobStorageRepository.DownloadBlobAsStreamAsync();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<AirportCsvMap>();

        await foreach (var record in csv.GetRecordsAsync<Airport>())
        {
            if (!string.IsNullOrWhiteSpace(record.Code) &&
                string.Equals(record.CountryCode, "US", StringComparison.OrdinalIgnoreCase) &&
                !ExcludedAirportTypes.Contains(record.AirportType))
            {
                yield return record;
            }
        }
    }
}