using CaptainPayment.Core.Models;
using DesolaDomain.Entities.Payment;
using DesolaServices.Commands.Requests;

namespace DesolaServices.Interfaces;

/// <summary>
/// Interface for setting up Desola subscription products and pricing (one-time setup)
/// </summary>
public interface IDesolaSubscriptionService
{
    /// <summary>
    /// Creates a subscription product for Desola
    /// </summary>
    Task<ProductResult> CreateSubscriptionProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a price for an existing product
    /// </summary>
    Task<PriceResult> CreateProductPriceAsync(CreatePriceRequest request, CancellationToken cancellationToken = default);

    Task<DesolaSubscriptionPlan> CreateSubscriptionPlanAsync(DesolaSubscriptionPlanRequest request, CancellationToken cancellationToken = default);

    Task<List<DesolaSubscriptionPlan>> GetSubscriptionPlansAsync(bool activeOnly = true, CancellationToken cancellationToken = default);


    Task<DesolaSubscriptionPlan> GetSubscriptionPlanAsync(string productId, string priceId, CancellationToken cancellationToken = default);

    Task<DesolaProductDetail> GetProductAsync(string productId, CancellationToken cancellationToken = default);

    Task<DesolaPriceDetail> GetPriceAsync(string productId, string priceId, CancellationToken cancellationToken = default);

    Task<List<DesolaPriceDetail>> GetProductPricesAsync(string productId, bool activeOnly = true, CancellationToken cancellationToken = default);

    Task<bool> UpdateProductDisplaySettingsAsync(string productId, UpdateProductDisplayRequest request, CancellationToken cancellationToken = default);

    Task<bool> UpdatePriceDisplaySettingsAsync(string productId, string priceId, UpdatePriceDisplayRequest request, CancellationToken cancellationToken = default);

    Task SyncFromStripeAsync(CancellationToken cancellationToken = default);

    Task SyncProductFromStripeAsync(string productId, CancellationToken cancellationToken = default);

    Task SyncPriceFromStripeAsync(string priceId, CancellationToken cancellationToken = default);
}