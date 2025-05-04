using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class ClickTrackingTableService: BaseTableStorage<UserClickTracking>
{
    public ClickTrackingTableService(
        ILogger<ClickTrackingTableService> logger,
        ICacheService cacheService,
        ITableStorageRepository<UserClickTracking> storageRepository,
        IOptions<AppSettings> configuration)
        : base(storageRepository, cacheService, logger, configuration.Value.Database.UserClickTrackingTableName)
    {
    }

    protected override string GetPartitionKey(UserClickTracking entity) => entity.PartitionKey;
    protected override string GetRowKey(UserClickTracking entity) => entity.RowKey;
    protected override ETag GetETag(UserClickTracking entity) => entity.ETag;
    protected override void SetETag(UserClickTracking entity, ETag etag) => entity.ETag = etag;
}