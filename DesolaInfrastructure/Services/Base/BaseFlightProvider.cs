using AutoMapper;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using DesolaInfrastructure.Services.FlightSorting;

namespace DesolaInfrastructure.Services.Base;

public abstract class BaseFlightProvider : IFlightProvider
{
    protected readonly IApiService ApiService;
    protected readonly ILogger Logger;
    protected readonly IMapper Mapper;
    protected readonly ICacheService MemoryCache;
    protected readonly IBlobStorageRepository BlobStorageRepository;
    private readonly string _flightCacheContainerName;

    protected BaseFlightProvider(ICacheService memoryCache, IMapper mapper, ILogger logger, IApiService apiService, IBlobStorageRepository blobStorageRepository, string flightCacheContainerName = "travels")
    {
        MemoryCache = memoryCache;
        Mapper = mapper;
        Logger = logger;
        ApiService = apiService;
        BlobStorageRepository = blobStorageRepository;
        _flightCacheContainerName = flightCacheContainerName;
    }
    public async Task<UnifiedFlightSearchResponse> SearchFlightsAsync(FlightSearchParameters parameters, CancellationToken cancellationToken)
    {
        var cacheKey = GenerateCacheKey(parameters);

        var memoryResult = CheckMemoryCache(cacheKey);
        if (memoryResult != null)
        {
            Logger.LogInformation($"Returning memory cached response for {ProviderName}: {cacheKey}");
            return memoryResult;
        }

        var blobResult = await CheckBlobCacheAsync(cacheKey);
        if (blobResult != null)
        {
            return blobResult;
        }

        try
        {
            var response = await GetFlightOffersAsync(parameters, cancellationToken);

            if (response.TotalResults <= 0)
            {
                return response;
            }

            ApplySorting(response, parameters.SortBy, parameters.SortOrder);

            MemoryCache.Add(cacheKey, response, TimeSpan.FromHours(10));

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error searching for flights with {ProviderName} provider");
            throw;
        }
    }

    public virtual string ProviderName { get; set; }

    protected abstract Task<UnifiedFlightSearchResponse> GetFlightOffersAsync(FlightSearchParameters parameters, CancellationToken cancellationToken);

    protected abstract string GenerateCacheKey(FlightSearchParameters parameters);

    protected virtual void ApplySorting(UnifiedFlightSearchResponse response, string sortBy, string sortOrder)
    {
        // Default implementation can call UnifiedFlightSortedResponse.ApplySorting
        UnifiedFlightSortedResponse.ApplySorting(response, sortBy, sortOrder);
    }

    private string GetBlobName(string cacheKey, string responseType) => $"{cacheKey}_{responseType}.json";

    protected void SaveToBlobCacheAsync<T>(string cacheKey, T rawResponse, UnifiedFlightSearchResponse unifiedResponse)
    {
        if(unifiedResponse.TotalResults <= 0)
        {
            Logger.LogInformation("No results found for {Provider}. Not saving to blob cache. Cache key: {CacheKey}", ProviderName, cacheKey);
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                var rawJson = JsonSerializer.Serialize(rawResponse, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var unifiedJson = JsonSerializer.Serialize(unifiedResponse, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                // Create blob names
                var rawBlobName = GetBlobName(cacheKey, "Raw");
                var unifiedBlobName = GetBlobName(cacheKey, "Unified");

                // Upload in parallel
                var rawUploadTask = BlobStorageRepository.UploadBlobAsync(
                    rawBlobName, rawJson, _flightCacheContainerName, "application/json");

                var unifiedUploadTask = BlobStorageRepository.UploadBlobAsync(
                    unifiedBlobName, unifiedJson, _flightCacheContainerName, "application/json");

                // Wait for both to complete (within the background task)
                await Task.WhenAll(rawUploadTask, unifiedUploadTask);

                Logger.LogInformation("Saved {Provider} responses to blob storage cache. Cache key: {CacheKey}",
                    ProviderName, cacheKey);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving {Provider} responses to blob storage cache. Cache key: {CacheKey}",
                    ProviderName, cacheKey);
            }
        }).ConfigureAwait(false);
    }

    protected UnifiedFlightSearchResponse CheckMemoryCache(string cacheKey)
    {
        if (!MemoryCache.Contains(cacheKey)) return null;
        var cachedResponse = MemoryCache.GetItem<UnifiedFlightSearchResponse>(cacheKey);
        if (cachedResponse == null) return null;
        Logger.LogInformation("Returning memory cached response for {Provider}. Cache key: {CacheKey}", ProviderName, cacheKey);
        return cachedResponse;
    }

    protected async Task<UnifiedFlightSearchResponse> CheckBlobCacheAsync(string cacheKey)
    {
        var blobName = GetBlobName(cacheKey, "Unified");

        try
        {
            if (await BlobStorageRepository.DoesBlobExistAsync(blobName, _flightCacheContainerName))
            {
                Logger.LogInformation("Found blob storage cache for {Provider}. Blob name: {BlobName}", ProviderName, blobName);

                var blobData = await BlobStorageRepository.DownloadBlobAsStringAsync(blobName, _flightCacheContainerName);

                if (!string.IsNullOrEmpty(blobData))
                {
                    var cachedResponse = JsonSerializer.Deserialize<UnifiedFlightSearchResponse>(blobData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (cachedResponse != null)
                    {
                        // Add to memory cache for future requests
                        MemoryCache.Add(cacheKey, cachedResponse, TimeSpan.FromHours(10));

                        Logger.LogInformation("Returning blob storage cached response for {Provider}. Blob name: {BlobName}", ProviderName, blobName);
                        return cachedResponse;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error checking blob cache for {Provider}. Cache key: {CacheKey}, Blob name: {BlobName}", ProviderName, cacheKey, blobName);
        }

        return null;
    }
}