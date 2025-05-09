﻿using System.Globalization;
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
    public AirportRepository(IBlobClientRepository blobStorageRepository, ICacheService cacheService)
    {
        _blobStorageRepository = blobStorageRepository;
        _cacheService = cacheService;
    }


    public async Task<IEnumerable<Airport>> SearchAirportsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Airport>();


        query = query.Trim().ToLowerInvariant();
        var airports = await GetAirportsAsync();

        return airports
            .Where(a =>
                (!string.IsNullOrEmpty(a.Name) && a.Name.ToLowerInvariant().Contains(query)) ||
                (!string.IsNullOrEmpty(a.City) && a.City.ToLowerInvariant().Contains(query)) ||
                (!string.IsNullOrEmpty(a.Code) && a.Code.ToLowerInvariant().Contains(query)))
            .Take(10);
    }

    public async Task<List<Airport>> GetAirportsAsync()
    {
        if (_cacheService.Contains(CacheEntry.AllAirports))
        {
            return _cacheService.GetItem<List<Airport>>(CacheEntry.AllAirports) ?? new List<Airport>();
        }

        var airports = await ReadUsAirportsAsync().ToListAsync();

        if (airports.Count > 0)
        {
            _cacheService.Add(CacheEntry.AllAirports, airports, TimeSpan.FromDays(30));
        }

        return airports;
    }

    public async Task<bool> IsAirportValidAsync(string airportCode)
    {
        if (_cacheService.Contains(CacheEntry.AllAirports))
        {
            var airports = _cacheService.GetItem<List<Airport>>(CacheEntry.AllAirports);
            return airports?.Any(airport => string.Equals(airport.Code, airportCode, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        return await ReadUsAirportsAsync()
            .AnyAsync(airport => string.Equals(airport.Code, airportCode, StringComparison.OrdinalIgnoreCase));
    }
    private async IAsyncEnumerable<Airport> ReadUsAirportsAsync()
    {
        if (!await _blobStorageRepository.DoesBlobExistAsync())
            yield break;

        await using var stream = await _blobStorageRepository.DownloadBlobAsStreamAsync();
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
            if (!string.IsNullOrWhiteSpace(record.Code) && string.Equals(record.CountryCode, "US", StringComparison.OrdinalIgnoreCase) &&  string.Equals(record.AirportType, "large_airport", StringComparison.OrdinalIgnoreCase))
            {
                yield return record;
            }
        }
    }
}