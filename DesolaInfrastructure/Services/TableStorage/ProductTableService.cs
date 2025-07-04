using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.Payment;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class ProductTableService : BaseTableStorage<DesolaProductDetail>
{
    public ProductTableService(
        ITableStorageRepository<DesolaProductDetail> storageRepository,
        ICacheService cacheService,
        ILogger<ProductTableService> logger,
        IOptions<AppSettings> configuration)
        : base(storageRepository, cacheService, logger,
            configuration.Value.Database.ProductsTableName)
    {
    }

    protected override string GetPartitionKey(DesolaProductDetail entity) => "products";
    protected override string GetRowKey(DesolaProductDetail entity) => entity.StripeProductId;
    protected override ETag GetETag(DesolaProductDetail entity) => entity.ETag;
    protected override void SetETag(DesolaProductDetail entity, ETag etag) => entity.ETag = etag;
}