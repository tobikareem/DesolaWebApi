using Azure;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Entities.Payment;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Services.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DesolaInfrastructure.Services.TableStorage;

public class PaymentIntentResultTableService : BaseTableStorage<PaymentIntentResult>
{
    public PaymentIntentResultTableService(
        ITableStorageRepository<PaymentIntentResult> storageRepository,
        ICacheService cacheService,
        ILogger<PaymentIntentResultTableService> logger,
        IOptions<AppSettings> configuration,
        TimeSpan? cacheExpiration = null)
        : base(storageRepository, cacheService, logger, configuration.Value.Database.PaymentIntentTableName, cacheExpiration)
    {
    }

    protected override string GetPartitionKey(PaymentIntentResult entity)
    {
        var timestamp = entity.Timestamp ?? DateTimeOffset.UtcNow;
        return $"payment_{timestamp:yyyy-MM}";
    }

    protected override string GetRowKey(PaymentIntentResult entity) => entity.Id;


    protected override ETag GetETag(PaymentIntentResult entity) => entity.ETag;

    protected override void SetETag(PaymentIntentResult entity, ETag etag)
    {
        entity.ETag = etag;
    }
}