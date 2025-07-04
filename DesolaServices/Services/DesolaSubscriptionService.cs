using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using CaptainPayment.Stripe.Extension;
using DesolaDomain.Entities.Payment;
using DesolaServices.Commands.Requests;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Services;

public class DesolaSubscriptionService : IDesolaSubscriptionService
{
    private readonly IProductService _productService;
    private readonly ILogger<DesolaSubscriptionService> _logger;
    private readonly IDesolaProductAndPriceStorage _desolaProductAndPriceStorage;
    public DesolaSubscriptionService(IProductService productService, ILogger<DesolaSubscriptionService> logger, IDesolaProductAndPriceStorage desolaProductAndPriceStorage)
    {
        _productService = productService;
        _logger = logger;
        _desolaProductAndPriceStorage = desolaProductAndPriceStorage;
    }


    public async Task<ProductResult> CreateSubscriptionProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating Desola subscription product: {ProductName}", request.Name);

        try
        {

            request.Metadata["created_by"] = "desola_setup";
            request.Metadata["product_type"] = "subscription";
            request.Metadata["created_at"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var stripeResult = await _productService.CreateProductAsync(request, cancellationToken);

            // 2. Store locally
            var localProduct = new DesolaProductDetail
            {
                StripeProductId = stripeResult.Id,
                Name = stripeResult.Name,
                Description = stripeResult.Description,
                IsActive = stripeResult.Active,
                ProductType = "subscription",
                Metadata = System.Text.Json.JsonSerializer.Serialize(stripeResult.Metadata),
                SyncStatus = "synced"
            };
            await _desolaProductAndPriceStorage.CreateProductAsync(localProduct);

            _logger.LogInformation("Created Desola product in Stripe and stored locally: {ProductId}", stripeResult.Id);
            return stripeResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription product: {ProductName}", request.Name);
            throw;
        }
    }

    public async Task<PriceResult> CreateProductPriceAsync(CreatePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating price for product {ProductId}: {Amount} {Currency} {Interval}",
            request.ProductId, request.UnitAmount, request.Currency, request.Recurring.Interval.GetDisplayName());

        try
        {

            // Add Desola metadata
            request.Metadata["created_by"] = "desola_setup";
            request.Metadata["created_at"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            request.Metadata["interval"] = request.Recurring.Interval.GetDisplayName();

            // 1. Create in Stripe
            var stripeResult = await _productService.CreatePriceAsync(request, cancellationToken);

            // 2. Store locally
            var localPrice = new DesolaPriceDetail
            {
                StripePriceId = stripeResult.Id,
                StripeProductId = stripeResult.ProductId,
                UnitAmountInCents = (long)(stripeResult.UnitAmount * 100), 
                Currency = stripeResult.Currency,
                Nickname = stripeResult.Nickname,
                IsActive = stripeResult.Active,
                BillingInterval = request.Recurring.Interval.GetDisplayName(),
                IntervalCount = request.Recurring.IntervalCount ?? 1,
                UsageType = request.Recurring.UsageType.GetDisplayName(),
                IsTrialEligible = true,
                TrialDays = 0,
                PromotionalTag = string.Empty,
                Metadata = System.Text.Json.JsonSerializer.Serialize(stripeResult.Metadata),
                SyncStatus = "synced"
            };

            await _desolaProductAndPriceStorage.CreatePriceAsync(localPrice);

            _logger.LogInformation("Created price in Stripe and stored locally: {PriceId}", stripeResult.Id);
            return stripeResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product price for product {ProductId}", request.ProductId);
            throw;
        }
    }

    public async Task<DesolaSubscriptionPlan> CreateSubscriptionPlanAsync(DesolaSubscriptionPlanRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating complete Desola subscription plan: {PlanName}", request.PlanName);

        try
        {
            // 1. Create Product
            var productRequest = new CreateProductRequest
            {
                Name = $"Desola {request.PlanName}",
                Description = request.Description,
                Active = true,
                Metadata = new Dictionary<string, string>
                {
                    ["plan_type"] = request.Interval.GetDisplayName(),
                    ["amount_cents"] = request.AmountInCents.ToString()
                }
            };

            var productResult = await CreateSubscriptionProductAsync(productRequest, cancellationToken);

            // 2. Create Price
            var priceRequest = new CreatePriceRequest
            {
                ProductId = productResult.Id,
                Currency = request.Currency,
                UnitAmount = request.AmountInCents,
                Active = true,
                Nickname = $"desola-{request.Interval.GetDisplayName()}-{request.AmountInCents}",
                Recurring = new RecurringOptions
                {
                    Interval = request.Interval,
                    IntervalCount = 1,
                    UsageType = StripeUsageType.Licensed
                }
            };

            var priceResult = await CreateProductPriceAsync(priceRequest, cancellationToken);

            // 3. Return combined plan
            var plan = new DesolaSubscriptionPlan
            {
                ProductId = productResult.Id,
                PriceId = priceResult.Id,
                ProductName = request.PlanName,
                Description = request.Description,
                AmountInCents = request.AmountInCents,
                Currency = request.Currency,
                BillingInterval = request.Interval.GetDisplayName(),
                FormattedPrice = $"${(request.AmountInCents / 100m):F2}/{request.Interval.GetDisplayName()}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Created complete subscription plan: Product={ProductId}, Price={PriceId}",
                productResult.Id, priceResult.Id);

            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription plan: {PlanName}", request.PlanName);
            throw;
        }
    }

    #region Retrieve Operations (From Local Storage)

    public async Task<List<DesolaSubscriptionPlan>> GetSubscriptionPlansAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving subscription plans, activeOnly: {ActiveOnly}", activeOnly);
            return await _desolaProductAndPriceStorage.GetAllSubscriptionPlansAsync(activeOnly);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plans");
            throw;
        }
    }

    public async Task<DesolaSubscriptionPlan> GetSubscriptionPlanAsync(string productId, string priceId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving subscription plan: Product={ProductId}, Price={PriceId}", productId, priceId);
            return await _desolaProductAndPriceStorage.GetSubscriptionPlanAsync(productId, priceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plan: Product={ProductId}, Price={PriceId}", productId, priceId);
            throw;
        }
    }

    public async Task<DesolaProductDetail> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _desolaProductAndPriceStorage.GetProductAsync(productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product: {ProductId}", productId);
            throw;
        }
    }

    public async Task<DesolaPriceDetail> GetPriceAsync(string productId, string priceId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _desolaProductAndPriceStorage.GetPriceAsync(productId, priceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price: {PriceId} for product: {ProductId}", priceId, productId);
            throw;
        }
    }

    public async Task<List<DesolaPriceDetail>> GetProductPricesAsync(string productId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _desolaProductAndPriceStorage.GetProductPricesAsync(productId, activeOnly);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices for product: {ProductId}", productId);
            throw;
        }
    }

    #endregion

    #region Update Operations

    public async Task<bool> UpdateProductDisplaySettingsAsync(string productId, UpdateProductDisplayRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating product display settings: {ProductId}", productId);

            var product = await _desolaProductAndPriceStorage.GetProductAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found for display update: {ProductId}", productId);
                return false;
            }

            await _desolaProductAndPriceStorage.UpdateProductAsync(product);

            _logger.LogInformation("Updated product display settings: {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product display settings: {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> UpdatePriceDisplaySettingsAsync(string productId, string priceId, UpdatePriceDisplayRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating price display settings: {PriceId} for product: {ProductId}", priceId, productId);

            var price = await _desolaProductAndPriceStorage.GetPriceAsync(productId, priceId);
            if (price == null)
            {
                _logger.LogWarning("Price not found for display update: {PriceId} for product: {ProductId}", priceId, productId);
                return false;
            }

            // Update display settings
            if (request.IsTrialEligible.HasValue)
                price.IsTrialEligible = request.IsTrialEligible.Value;

            if (request.TrialDays.HasValue)
                price.TrialDays = request.TrialDays.Value;

            if (!string.IsNullOrWhiteSpace(request.PromotionalTag))
                price.PromotionalTag = request.PromotionalTag;

            if (request.IsActive.HasValue)
                price.IsActive = request.IsActive.Value;

            await _desolaProductAndPriceStorage.UpdatePriceAsync(price);

            _logger.LogInformation("Updated price display settings: {PriceId} for product: {ProductId}", priceId, productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price display settings: {PriceId} for product: {ProductId}", priceId, productId);
            throw;
        }
    }

    #endregion

    #region Sync Operations

    public async Task SyncFromStripeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting sync from Stripe");
            await _desolaProductAndPriceStorage.SyncAllFromStripeAsync();
            _logger.LogInformation("Completed sync from Stripe");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Stripe sync");
            throw;
        }
    }

    public async Task SyncProductFromStripeAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get latest from Stripe
            var stripeProduct = await _productService.GetProductAsync(productId, cancellationToken);

            // Update local storage
            var localProduct = await _desolaProductAndPriceStorage.GetProductAsync(productId);
            if (localProduct != null)
            {
                localProduct.Name = stripeProduct.Name;
                localProduct.Description = stripeProduct.Description;
                localProduct.IsActive = stripeProduct.Active;
                localProduct.Metadata = System.Text.Json.JsonSerializer.Serialize(stripeProduct.Metadata);
                localProduct.LastSyncedFromStripe = DateTime.UtcNow;
                localProduct.SyncStatus = "synced";

                await _desolaProductAndPriceStorage.UpdateProductAsync(localProduct);
                _logger.LogInformation("Synced product from Stripe: {ProductId}", productId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing product from Stripe: {ProductId}", productId);
            throw;
        }
    }

    public async Task SyncPriceFromStripeAsync(string priceId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get latest from Stripe
            var stripePrice = await _productService.GetPriceAsync(priceId, cancellationToken);

            // Update local storage
            var localPrice = await _desolaProductAndPriceStorage.GetPriceAsync(stripePrice.ProductId, priceId);
            if (localPrice != null)
            {
                localPrice.UnitAmountInCents = (long)(stripePrice.UnitAmount * 100);
                localPrice.Currency = stripePrice.Currency;
                localPrice.Nickname = stripePrice.Nickname;
                localPrice.IsActive = stripePrice.Active;
                localPrice.Metadata = System.Text.Json.JsonSerializer.Serialize(stripePrice.Metadata);
                localPrice.LastSyncedFromStripe = DateTime.UtcNow;
                localPrice.SyncStatus = "synced";

                await _desolaProductAndPriceStorage.UpdatePriceAsync(localPrice);
                _logger.LogInformation("Synced price from Stripe: {PriceId}", priceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing price from Stripe: {PriceId}", priceId);
            throw;
        }
    }

    #endregion
}