using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaServices.Services;

public class UserProfileService : ITableBase<UserTravelPreference>
{
    private readonly ITableStorageRepository<UserTravelPreference> _storageRepository;
    private readonly IAirportRepository _airportRepository;
    private readonly string _tableName;
    private readonly ICacheService _cacheService;
    private readonly ILogger<WebPageContentService> _logger;
    private bool _tableInitialized;

    public UserProfileService(
        ITableStorageRepository<UserTravelPreference> storageRepository,
        IAirportRepository airportRepository,
        ICacheService cacheService,
        ILogger<WebPageContentService> logger, IOptions<AppSettings> configuration)
    {
        _storageRepository = storageRepository;
        _airportRepository = airportRepository;
        _tableName = configuration.Value.Database.UserTravelPreferenceTableName;
        _cacheService = cacheService;
        _logger = logger;
    }

    private async Task EnsureTableExistsAsync()
    {
        if (!_tableInitialized)
        {
            await _storageRepository.CreateTableAsync(_tableName);
            _tableInitialized = true;
        }
    }

    public async Task InsertTableEntityAsync(UserTravelPreference entity)
    {
        if (string.IsNullOrWhiteSpace(entity.UserId))
            throw new ArgumentNullException(nameof(entity.UserId), "UserId is required for UserTravelPreference.");

        await ValidateAirportCodesAsync(entity.OriginAirport, entity.DestinationAirport);

        await EnsureTableExistsAsync();
        await _storageRepository.InsertTableEntityAsync(_tableName, entity);
        
        var cacheKey = GetCacheKey(entity.PartitionKey, entity.RowKey);
        _cacheService.Add(cacheKey, entity, TimeSpan.FromMinutes(30));
    }


    public async Task<UserTravelPreference> GetTableEntityAsync(string partitionKey, string rowKey)
    {
        var cacheKey = GetCacheKey(partitionKey, rowKey);
        
        var cachedEntity = _cacheService.GetItem<UserTravelPreference>(cacheKey);
        if (cachedEntity != null)
        {
            _logger.LogInformation($"Cache hit for PartitionKey: {partitionKey}, RowKey: {rowKey}");
            return cachedEntity;
        }

        try
        {
            var entity = await _storageRepository.GetTableEntityAsync(_tableName, partitionKey, rowKey);
            _cacheService.Add(cacheKey, entity, TimeSpan.FromMinutes(30));

            return entity;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404) 
        {
            _logger.LogWarning($"Entity not found: PartitionKey '{partitionKey}', RowKey '{rowKey}'");
            return new UserTravelPreference(); 
        }
    }


    public async Task UpdateTableEntityAsync(UserTravelPreference entity)
    {
        if (string.IsNullOrEmpty(entity.UserId))
            throw new ArgumentException("UserId must be set");

        await EnsureTableExistsAsync();

        var existing = await GetTableEntityAsync(entity.PartitionKey, entity.RowKey);
        if (existing == null)
            throw new Exception($"Entity with UserId '{entity.PartitionKey}' and RowKey '{entity.RowKey}' not found.");

        entity.ETag = existing.ETag;
        await _storageRepository.UpdateTableEntityAsync(_tableName, entity);
        _cacheService.Remove(GetCacheKey(entity.PartitionKey, entity.RowKey));
    }


    private string GetCacheKey(string partitionKey, string rowKey) => $"{partitionKey}:{rowKey}";

    private async Task ValidateAirportCodesAsync(string originAirportCode, string destinationAirportCode)
    {
        if (string.IsNullOrWhiteSpace(originAirportCode) || originAirportCode.Length < 3)
            throw new ArgumentNullException(nameof(originAirportCode), "Origin Airport code is required.");

        if (string.IsNullOrWhiteSpace(destinationAirportCode) || destinationAirportCode.Length < 3)
            throw new ArgumentNullException(nameof(destinationAirportCode), "Destination Airport code is required.");

        var originAirport = await _airportRepository.IsAirportValidAsync(originAirportCode);
        var destinationAirport = await _airportRepository.IsAirportValidAsync(destinationAirportCode);

        if (!originAirport || !destinationAirport)
            throw new ArgumentException("Invalid airport code.");

    }
}