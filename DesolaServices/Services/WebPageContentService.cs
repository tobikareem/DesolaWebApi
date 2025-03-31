using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.Pages;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaServices.Services
{
    public class WebPageContentService : ITableBase<WebSection>
    {
        private readonly ITableStorageRepository<WebSection> _storageRepository;
        private readonly string _tableName;
        private readonly ICacheService _cacheService;
        private readonly ILogger<WebPageContentService> _logger;
        private bool _tableInitialized;

        public WebPageContentService(
            ITableStorageRepository<WebSection> storageRepository,
            IOptions<AppSettings> configuration,
            ICacheService cacheService,
            ILogger<WebPageContentService> logger)
        {
            _storageRepository = storageRepository;
            _cacheService = cacheService;
            _logger = logger;
            _tableName = configuration.Value.Database.WebPageContentTableName;
        }

        private async Task EnsureTableExistsAsync()
        {
            if (!_tableInitialized)
            {
                await _storageRepository.CreateTableAsync(_tableName);
                _tableInitialized = true;
            }
        }

        public async Task InsertTableEntityAsync(WebSection entity)
        {
            await EnsureTableExistsAsync();

            await _storageRepository.InsertTableEntityAsync(_tableName, entity);

            // Add to cache
            var cacheKey = GetCacheKey(entity.PartitionKey, entity.RowKey);
            _cacheService.Add(cacheKey, entity, TimeSpan.FromMinutes(30));
        }

        public async Task<WebSection> GetTableEntityAsync(string partitionKey, string rowKey)
        {
            var cacheKey = GetCacheKey(partitionKey, rowKey);

            // Try retrieving from cache
            var cachedEntity = _cacheService.GetItem<WebSection>(cacheKey);
            if (cachedEntity != null)
            {
                _logger.LogInformation($"Cache hit for PartitionKey: {partitionKey}, RowKey: {rowKey}");
                return cachedEntity;
            }

            // Fetch from table storage
            var entity = await _storageRepository.GetTableEntityAsync(_tableName, partitionKey, rowKey);
            if (entity != null)
            {
                _cacheService.Add(cacheKey, entity, TimeSpan.FromMinutes(30));
            }

            return entity;
        }

        public async Task UpdateTableEntityAsync(WebSection entity)
        {
            await EnsureTableExistsAsync();

            var existing = await GetTableEntityAsync(entity.PartitionKey, entity.RowKey);
            if (existing == null)
            {
                throw new Exception($"Entity with PartitionKey '{entity.PartitionKey}' and RowKey '{entity.RowKey}' not found.");
            }

            entity.ETag = existing.ETag; // Ensure ETag is set correctly
            await _storageRepository.UpdateTableEntityAsync(_tableName, entity);

            _cacheService.Remove(GetCacheKey(entity.PartitionKey, entity.RowKey));
        }

        private string GetCacheKey(string partitionKey, string rowKey) => $"{partitionKey}:{rowKey}";
    }
}
