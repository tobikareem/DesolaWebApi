﻿using System.Text.Json;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using DesolaDomain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Data;

public class AirlineRepository : IAirlineRepository
{
    private readonly IBlobStorageRepository _blobStorageRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<AirlineRepository> _logger;

    private readonly string _fileName;
    private readonly string _containerName;
    private readonly string _americanAirlines;

    public AirlineRepository(IBlobStorageRepository blobStorageRepository, ICacheService cacheService, ILogger<AirlineRepository> logger, IOptions<AppSettings> configuration)
    {
        _blobStorageRepository = blobStorageRepository;
        _cacheService = cacheService;
        _logger = logger;
        var appSettings = configuration.Value;

        _fileName = appSettings.BlobFiles.AirportCodeFile ?? throw new ArgumentNullException(nameof(configuration), "Unable to find airline file name");
        _containerName = appSettings.StorageAccount.ContainerName ?? throw new ArgumentNullException(nameof(configuration), "Unable to find airline container name");
        _americanAirlines = appSettings.Airlines.UnitedStatesAirlines?? throw new ArgumentNullException(nameof(configuration), "Unable to find american airlines");
    }

    public async Task<List<Airline>> GetAllAsync()
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

        var airlineList = await ReadAirlineAsync().ToListAsync();

        _cacheService.Add(CacheEntry.AllAirlines, airlineList, TimeSpan.FromDays(30));

        return airlineList;

    }

    public async Task<Airline> GetByCodeAsync(string iataCode)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
        {
            throw new ArgumentException("IATA code must be provided", nameof(iataCode));
        }

        var airlineCache = _cacheService.GetItem<IEnumerable<Airline>>(CacheEntry.AllAirlines);

        if (airlineCache != null)
        {
            return airlineCache.FirstOrDefault(airline => string.Equals(airline.IataCode, iataCode, StringComparison.OrdinalIgnoreCase));
        }

        return await ReadAirlineAsync()
            .FirstOrDefaultAsync(airline => string.Equals(airline.IataCode, iataCode, StringComparison.OrdinalIgnoreCase));
    }

    private async IAsyncEnumerable<Airline> ReadAirlineAsync()
    {
        if (!await _blobStorageRepository.DoesBlobExistAsync(_fileName, _containerName))
            yield break;

        var stream = await _blobStorageRepository.DownloadBlobAsStreamAsync(_fileName, _containerName);

        await foreach (var airline in JsonSerializer.DeserializeAsyncEnumerable<Airline>(stream))
        {
            if (airline != null)
            {
                yield return airline;
            }
        }
    }
}