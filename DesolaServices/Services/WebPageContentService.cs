using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.PageEntity;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DesolaServices.Services
{
    public class WebPageContentService : ITableBase<WebSection>
    {
        private readonly ITableStorageRepository<WebSection> _storageRepository;
        private readonly string _tableName;
        public WebPageContentService(ITableStorageRepository<WebSection> storageRepository, IConfiguration configuration)
        {
            _storageRepository = storageRepository;
            _tableName = configuration["WebPageContentTableName"];
        }
        private async Task CreateAsync()
        {
            await _storageRepository.CreateTableAsync(_tableName);
        }

        public async Task InsertTableEntityAsync(WebSection entity)
        {
            await CreateAsync();
            await _storageRepository.InsertTableEntityAsync(_tableName, entity);
        }

        public async Task<WebSection> GetTableEntityAsync(string partitionKey, string rowKey)
        {
            var entity = await _storageRepository.GetTableEntityAsync(_tableName, partitionKey, rowKey);

            return entity;
        }

        public async Task UpdateTableEntityAsync(WebSection entity)
        {
            var existing = await GetTableEntityAsync(entity.PartitionKey, entity.RowKey);

            if (existing == null)
            {
                throw new Exception($"Entity with PartitionKey '{entity.PartitionKey}' and RowKey '{entity.RowKey}' not found.");

            }
            entity.ETag = existing.ETag;
            await _storageRepository.UpdateTableEntityAsync(_tableName, entity);

        }
    }
}
