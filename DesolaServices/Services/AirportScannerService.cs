using AutoMapper;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Web;
using DesolaDomain.Settings;
using Microsoft.Extensions.Options;

namespace DesolaServices.Services;

public class AirportScannerService : IAirportScannerService
{
    private readonly ICacheService _cacheService;
    private readonly IBlobStorageRepository _blobStorageRepository;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AirportScannerService> _logger;
    private readonly IApiService _apiService;
    private readonly IMapper _mapper;

    public AirportScannerService(
        ICacheService cacheService,
        IBlobStorageRepository blobClientRepository,
        IOptions<AppSettings> configuration,
        ILogger<AirportScannerService> logger,
        IApiService apiService,
        IMapper mapper)
    {
        _cacheService = cacheService;
        _logger = logger;
        _blobStorageRepository = blobClientRepository;
        _appSettings = configuration.Value ?? throw new ArgumentNullException(nameof(configuration), "AppSettings is null");
        _apiService = apiService;
        _mapper = mapper;
    }

    public async Task<List<AirportAutoCompleteResponse>> GetAutocompleteResultsAsync(string query, CancellationToken cancellationToken)
    {
        var airportScanners = GetAirportScannerFromCache();
        _logger.LogInformation(airportScanners.Any() ? "Obtained airports from cache" : "No Airport data in cache");

        var hasAirport = airportScanners.Any(x => x.City == query || x.Code == query || x.Name == query || x.Identity == query);

        if (hasAirport)
        {
            return airportScanners.Where(x => x.City == query || x.Code == query || x.Name == query || x.Identity == query).ToList();
        }

        airportScanners = await GetAirportScannerFromBlob();
        _logger.LogInformation(airportScanners.Any() ? "Obtained airports from blob" : "No Airport data in blob");

        hasAirport = airportScanners.Any(x => x.City == query || x.Code == query || x.Name == query || x.Identity == query);

        if (hasAirport)
        {
            // update cache data;
            _cacheService.Add(CacheEntry.GetAirportsFromSkyScanner, airportScanners, TimeSpan.FromDays(30));
            return airportScanners.Where(x => x.City == query || x.Code == query || x.Name == query || x.Identity == query).ToList();
        }

        airportScanners = await GetAirportsFromSkyScannerApi(query, cancellationToken);

        foreach (var airportInfo in airportScanners)
        {
            airportInfo.Identity = query;
        }

        _cacheService.Add(CacheEntry.GetAirportsFromSkyScanner, airportScanners, TimeSpan.FromDays(30));

        await UpsertAirportScannerToBlob(airportScanners);

        return airportScanners;
    }

    private async Task UpsertAirportScannerToBlob(IEnumerable<AirportAutoCompleteResponse> airportScanners)
    {
        var fileName = _appSettings.BlobFiles.SkyScannerAirportFile;
        var containerName = _appSettings.StorageAccount.ContainerName;

        var existingAirports = await GetAirportScannerFromBlob();
        existingAirports.AddRange(airportScanners);

        var content = JsonConvert.SerializeObject(existingAirports);

        await _blobStorageRepository.UploadBlobAsync(fileName, content, containerName, "application/json");

    }

    private List<AirportAutoCompleteResponse> GetAirportScannerFromCache()
    {
        return _cacheService.Contains(CacheEntry.GetAirportsFromSkyScanner) ? _cacheService.GetItem<List<AirportAutoCompleteResponse>>(CacheEntry.GetAirportsFromSkyScanner) : new List<AirportAutoCompleteResponse>();
    }

    private async Task<List<AirportAutoCompleteResponse>> GetAirportScannerFromBlob()
    {
        var fileName = _appSettings.BlobFiles.SkyScannerAirportFile;
        var containerName = _appSettings.StorageAccount.ContainerName;

        var hasAirportData = await _blobStorageRepository.DoesBlobExistAsync(fileName, containerName);

        List<AirportAutoCompleteResponse> airportScanners = new();
        if (!hasAirportData)
        {
            return airportScanners;
        }

        var stream = await _blobStorageRepository.DownloadBlobAsStreamAsync(fileName, containerName);
        airportScanners = await GetAirportsAsync(stream);

        return airportScanners;
    }

    private async Task<List<AirportAutoCompleteResponse>> GetAirportsFromSkyScannerApi(string query, CancellationToken cancellationToken)
    {
        var uri = BuildAutocompleteUri(query);

        var request = new HttpRequestMessage(HttpMethod.Get, uri)
        {
            Headers =
            {
                { "x-rapidapi-key", _appSettings.ExternalApi.RapidApi.SkyScannerKey },
                { "x-rapidapi-host",_appSettings.ExternalApi.RapidApi.SkyScannerHost  }
            }
        };

        var airportScanners = await _apiService.SendAsync<AirportScanner>(request, cancellationToken);

        var response = _mapper.Map<List<AirportAutoCompleteResponse>>(airportScanners.Data);
        return response;
    }

    private static async Task<List<AirportAutoCompleteResponse>> GetAirportsAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        var airportScanners = JsonConvert.DeserializeObject<List<AirportAutoCompleteResponse>>(json);
        return airportScanners;
    }

    private Uri BuildAutocompleteUri(string query)
    {
        var builder = new UriBuilder($"{_appSettings.ExternalApi.RapidApi.SkyScannerUri}/auto-complete");
        var queryParameters = HttpUtility.ParseQueryString(string.Empty);
        queryParameters["query"] = query;
        builder.Query = queryParameters.ToString() ?? string.Empty;
        return builder.Uri;
    }
}
