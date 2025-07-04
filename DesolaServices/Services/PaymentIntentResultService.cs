using CaptainPayment.Core.Models;
using DesolaDomain.Entities.Payment;
using DesolaDomain.Interfaces;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Services;

public class PaymentIntentResultService : IPaymentIntentResultService
{
    private readonly ICacheService _cacheService;
    private readonly ITableBase<PaymentIntentResult> _tableService;
    private readonly ILogger<PaymentIntentResultService> _logger;

    public PaymentIntentResultService(ICacheService cacheService, ITableBase<PaymentIntentResult> tableService, ILogger<PaymentIntentResultService> logger)
    {
        _cacheService = cacheService;
        _tableService = tableService;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentIntentResult>> GetByCustomerIdAsync(string customerId, int monthsBack = 12)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be null or empty", nameof(customerId));

        var results = new List<PaymentIntentResult>();
        var endDate = DateTimeOffset.UtcNow;
        var startDate = endDate.AddMonths(-monthsBack);

        for (var date = endDate; date >= startDate; date = date.AddMonths(-1))
        {
            var partitionKey = $"payment_{date:yyyy-MM}";
            var query = $"PartitionKey eq '{partitionKey}' and CustomerId eq '{customerId}'";

            try
            {
                var (partitionResults, _) = await _tableService.GetTableEntitiesByQueryAsync(query, 100, null);
                results.AddRange(partitionResults);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error querying partition {PartitionKey} for customer {CustomerId}", partitionKey, customerId);
            }
        }

        _logger.LogInformation("Found {Count} payment intents for customer {CustomerId}", results.Count, customerId);
        return results.OrderByDescending(x => x.Timestamp);
    }

    public async Task SavePaymentIntentAsync(SetupIntentResult setupIntent, string userId)
    {
        if (setupIntent == null)
            throw new ArgumentNullException(nameof(setupIntent));
        
        var paymentIntent = new PaymentIntentResult
        {
            Id = setupIntent.Id,
            ClientSecret = setupIntent.ClientSecret,
            Status = setupIntent.Status,
            PaymentMethodId = setupIntent.PaymentMethodId,
            CustomerId = setupIntent.CustomerId,
            UserId = userId,
        };

        paymentIntent.Timestamp ??= DateTimeOffset.UtcNow;
        
        paymentIntent.PartitionKey = $"payment_{paymentIntent.Timestamp:yyyy-MM}";
        paymentIntent.RowKey = paymentIntent.Id;

        await _tableService.InsertTableEntityAsync(paymentIntent);

        var cacheKey = $"setup_intent_{paymentIntent.Id}";
        _cacheService.Add(cacheKey, paymentIntent, TimeSpan.FromHours(24));

        _logger.LogInformation("Saved payment intent {SetupIntentId} for customer {CustomerId}", paymentIntent.Id, paymentIntent.CustomerId);
    }
}