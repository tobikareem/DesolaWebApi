using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.Pages;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class WebPageDesignTableService: BaseTableStorage<WebSection>
{
    public WebPageDesignTableService(
        ITableStorageRepository<WebSection> storageRepository,
        IOptions<AppSettings> configuration,
        ICacheService cacheService,
        ILogger<WebPageDesignTableService> logger)
        : base(storageRepository, cacheService, logger, configuration.Value.Database.WebPageContentTableName)
    {
    }

    protected override string GetPartitionKey(WebSection entity) => entity.PartitionKey;
    protected override string GetRowKey(WebSection entity) => entity.RowKey;
    protected override ETag GetETag(WebSection entity) => entity.ETag;
    protected override void SetETag(WebSection entity, ETag etag) => entity.ETag = etag;
}