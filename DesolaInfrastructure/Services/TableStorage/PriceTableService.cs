using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.Payment;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class PriceTableService : BaseTableStorage<DesolaPriceDetail>
{
    public PriceTableService(
        ITableStorageRepository<DesolaPriceDetail> storageRepository,
        ICacheService cacheService,
        ILogger<PriceTableService> logger,
        IOptions<AppSettings> configuration)
        : base(storageRepository, cacheService, logger,
            configuration.Value.Database.PricesTableName)
    {
    }

    protected override string GetPartitionKey(DesolaPriceDetail entity) => entity.StripeProductId;
    protected override string GetRowKey(DesolaPriceDetail entity) => entity.StripePriceId;
    protected override ETag GetETag(DesolaPriceDetail entity) => entity.ETag;
    protected override void SetETag(DesolaPriceDetail entity, ETag etag) => entity.ETag = etag;
}