using DesolaDomain.Entities.Payment;
using DesolaDomain.Interfaces;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Services;

public class DesolaProductAndPriceStorage : IDesolaProductAndPriceStorage
{
    private readonly ITableBase<DesolaProductDetail> _productStorage;
    private readonly ITableBase<DesolaPriceDetail> _priceStorage;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DesolaProductAndPriceStorage> _logger;

    public DesolaProductAndPriceStorage(
        ITableBase<DesolaProductDetail> productStorage,
        ITableBase<DesolaPriceDetail> priceStorage,
        ICacheService cacheService,
        ILogger<DesolaProductAndPriceStorage> logger)
    {
        _productStorage = productStorage;
        _priceStorage = priceStorage;
        _cacheService = cacheService;
        _logger = logger;
    }

    #region Product Operations

    public async Task<DesolaProductDetail> CreateProductAsync(DesolaProductDetail product)
    {
        product.PartitionKey = "products";
        product.RowKey = product.StripeProductId;
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.LastSyncedFromStripe = DateTime.UtcNow;

        await _productStorage.InsertTableEntityAsync(product);

        _cacheService.Remove("all_products");
        _cacheService.Remove("all_products_active");
        _logger.LogInformation("Created product: {ProductId}", product.StripeProductId);

        return product;
    }

    public async Task<DesolaProductDetail> GetProductAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            var product = await _productStorage.GetTableEntityAsync("products", productId);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product: {ProductId}", productId);
            throw;
        }
    }

    public async Task<DesolaProductDetail> UpdateProductAsync(DesolaProductDetail product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        try
        {
            product.UpdatedAt = DateTime.UtcNow;
            product.LastSyncedFromStripe = DateTime.UtcNow;

            await _productStorage.UpdateTableEntityAsync(product);

            // Clear caches
            _cacheService.Remove("all_products");
            _cacheService.Remove("all_products_active");
            _cacheService.Remove("all_subscription_plans_True");
            _cacheService.Remove("all_subscription_plans_False");

            _logger.LogInformation("Updated product: {ProductId}", product.StripeProductId);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", product.StripeProductId);
            throw;
        }
    }

    public async Task<List<DesolaProductDetail>> GetAllProductsAsync(bool activeOnly = true)
    {
        var cacheKey = $"all_products_{activeOnly}";
        var cached = _cacheService.GetItem<List<DesolaProductDetail>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning cached products, activeOnly: {ActiveOnly}", activeOnly);
            return cached;
        }

        try
        {
            var query = "PartitionKey eq 'products'";
            if (activeOnly)
                query += " and IsActive eq true";

            var (products, _) = await _productStorage.GetTableEntitiesByQueryAsync(query, 100, null);

            _cacheService.Add(cacheKey, products, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {Count} products, activeOnly: {ActiveOnly}",
                products.Count, activeOnly);

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products, activeOnly: {ActiveOnly}", activeOnly);
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            var product = await GetProductAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found for deletion: {ProductId}", productId);
                return false;
            }

            // Soft delete - mark as inactive
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _productStorage.UpdateTableEntityAsync(product);

            // Clear caches
            _cacheService.Remove("all_products");
            _cacheService.Remove("all_products_active");
            _cacheService.Remove("all_subscription_plans_True");
            _cacheService.Remove("all_subscription_plans_False");

            _logger.LogInformation("Soft deleted product: {ProductId}", productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", productId);
            throw;
        }
    }

    #endregion

    #region Price Operations

    public async Task<DesolaPriceDetail> CreatePriceAsync(DesolaPriceDetail price)
    {
        price.PartitionKey = price.StripeProductId; // Group by product
        price.RowKey = price.StripePriceId;
        price.CreatedAt = DateTime.UtcNow;
        price.UpdatedAt = DateTime.UtcNow;
        price.LastSyncedFromStripe = DateTime.UtcNow;

        await _priceStorage.InsertTableEntityAsync(price);

        // Clear caches
        _cacheService.Remove($"product_prices_{price.StripeProductId}_True");
        _cacheService.Remove($"product_prices_{price.StripeProductId}_False");
        _cacheService.Remove("all_prices_True");
        _cacheService.Remove("all_prices_False");
        _cacheService.Remove("all_subscription_plans_True");
        _cacheService.Remove("all_subscription_plans_False");

        _logger.LogInformation("Created price: {PriceId} for product: {ProductId}",
            price.StripePriceId, price.StripeProductId);

        return price;
    }

    public async Task<DesolaPriceDetail> GetPriceAsync(string productId, string priceId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(priceId))
            throw new ArgumentException("Price ID cannot be null or empty", nameof(priceId));

        try
        {
            var price = await _priceStorage.GetTableEntityAsync(productId, priceId);
            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving price: {PriceId} for product: {ProductId}",
                priceId, productId);
            throw;
        }
    }

    public async Task<DesolaPriceDetail> UpdatePriceAsync(DesolaPriceDetail price)
    {
        if (price == null)
            throw new ArgumentNullException(nameof(price));

        try
        {
            price.UpdatedAt = DateTime.UtcNow;
            price.LastSyncedFromStripe = DateTime.UtcNow;

            await _priceStorage.UpdateTableEntityAsync(price);

            // Clear caches
            _cacheService.Remove($"product_prices_{price.StripeProductId}_True");
            _cacheService.Remove($"product_prices_{price.StripeProductId}_False");
            _cacheService.Remove("all_prices_True");
            _cacheService.Remove("all_prices_False");
            _cacheService.Remove("all_subscription_plans_True");
            _cacheService.Remove("all_subscription_plans_False");

            _logger.LogInformation("Updated price: {PriceId} for product: {ProductId}",
                price.StripePriceId, price.StripeProductId);

            return price;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price: {PriceId} for product: {ProductId}",
                price.StripePriceId, price.StripeProductId);
            throw;
        }
    }

    public async Task<List<DesolaPriceDetail>> GetProductPricesAsync(string productId, bool activeOnly = true)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        var cacheKey = $"product_prices_{productId}_{activeOnly}";
        var cached = _cacheService.GetItem<List<DesolaPriceDetail>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning cached prices for product: {ProductId}, activeOnly: {ActiveOnly}",
                productId, activeOnly);
            return cached;
        }

        try
        {
            var query = $"PartitionKey eq '{productId}'";
            if (activeOnly)
                query += " and IsActive eq true";

            var (prices, _) = await _priceStorage.GetTableEntitiesByQueryAsync(query, 100, null);

            var sortedPrices = prices.OrderBy(p => p.UnitAmountInCents).ToList();

            _cacheService.Add(cacheKey, sortedPrices, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {Count} prices for product: {ProductId}, activeOnly: {ActiveOnly}",
                sortedPrices.Count, productId, activeOnly);

            return sortedPrices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices for product: {ProductId}, activeOnly: {ActiveOnly}",
                productId, activeOnly);
            throw;
        }
    }

    public async Task<List<DesolaPriceDetail>> GetAllPricesAsync(bool activeOnly = true)
    {
        var cacheKey = $"all_prices_{activeOnly}";
        var cached = _cacheService.GetItem<List<DesolaPriceDetail>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning cached all prices, activeOnly: {ActiveOnly}", activeOnly);
            return cached;
        }

        try
        {
            // Get all products first to know which partitions to query
            var products = await GetAllProductsAsync(false); // Get all products regardless of active status
            var allPrices = new List<DesolaPriceDetail>();

            foreach (var product in products)
            {
                var productPrices = await GetProductPricesAsync(product.StripeProductId, activeOnly);
                allPrices.AddRange(productPrices);
            }

            var sortedPrices = allPrices.OrderBy(p => p.StripeProductId)
                                      .ThenBy(p => p.UnitAmountInCents)
                                      .ToList();

            _cacheService.Add(cacheKey, sortedPrices, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {Count} total prices, activeOnly: {ActiveOnly}",
                sortedPrices.Count, activeOnly);

            return sortedPrices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all prices, activeOnly: {ActiveOnly}", activeOnly);
            throw;
        }
    }

    public async Task<bool> DeletePriceAsync(string productId, string priceId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(priceId))
            throw new ArgumentException("Price ID cannot be null or empty", nameof(priceId));

        try
        {
            var price = await GetPriceAsync(productId, priceId);
            if (price == null)
            {
                _logger.LogWarning("Price not found for deletion: {PriceId} for product: {ProductId}",
                    priceId, productId);
                return false;
            }

            // Soft delete - mark as inactive
            price.IsActive = false;
            price.UpdatedAt = DateTime.UtcNow;

            await _priceStorage.UpdateTableEntityAsync(price);

            // Clear caches
            _cacheService.Remove($"product_prices_{productId}_True");
            _cacheService.Remove($"product_prices_{productId}_False");
            _cacheService.Remove("all_prices_True");
            _cacheService.Remove("all_prices_False");
            _cacheService.Remove("all_subscription_plans_True");
            _cacheService.Remove("all_subscription_plans_False");

            _logger.LogInformation("Soft deleted price: {PriceId} for product: {ProductId}",
                priceId, productId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting price: {PriceId} for product: {ProductId}",
                priceId, productId);
            throw;
        }
    }

    #endregion

    #region Combined Operations

    public async Task<DesolaSubscriptionPlan> GetSubscriptionPlanAsync(string productId, string priceId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(priceId))
            throw new ArgumentException("Price ID cannot be null or empty", nameof(priceId));

        try
        {
            var product = await GetProductAsync(productId);
            var price = await GetPriceAsync(productId, priceId);

            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                return null;
            }

            if (price == null)
            {
                _logger.LogWarning("Price not found: {PriceId} for product: {ProductId}", priceId, productId);
                return null;
            }

            return new DesolaSubscriptionPlan
            {
                ProductId = product.StripeProductId,
                ProductName = product.Name,
                Description = product.Description,
                PriceId = price.StripePriceId,
                AmountInCents = price.UnitAmountInCents,
                Currency = price.Currency,
                BillingInterval = price.BillingInterval,
                FormattedPrice = price.FormattedPrice,
                PromotionalTag = price.PromotionalTag,
                IsTrialEligible = price.IsTrialEligible,
                TrialDays = price.TrialDays,

                IsActive = product.IsActive && price.IsActive,
                CreatedAt = product.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription plan: Product={ProductId}, Price={PriceId}",
                productId, priceId);
            throw;
        }
    }

    public async Task<List<DesolaSubscriptionPlan>> GetAllSubscriptionPlansAsync(bool activeOnly = true)
    {
        var cacheKey = $"all_subscription_plans_{activeOnly}";
        var cached = _cacheService.GetItem<List<DesolaSubscriptionPlan>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning cached subscription plans, activeOnly: {ActiveOnly}", activeOnly);
            return cached;
        }

        try
        {
            var products = await GetAllProductsAsync(activeOnly);
            var plans = new List<DesolaSubscriptionPlan>();

            foreach (var product in products)
            {
                var prices = await GetProductPricesAsync(product.StripeProductId, activeOnly);

                plans.AddRange(prices.Select(price => new DesolaSubscriptionPlan
                {
                    ProductId = product.StripeProductId,
                    ProductName = product.Name,
                    Description = product.Description,

                    PriceId = price.StripePriceId,
                    AmountInCents = price.UnitAmountInCents,
                    Currency = price.Currency,
                    BillingInterval = price.BillingInterval,
                    FormattedPrice = price.FormattedPrice,
                    PromotionalTag = price.PromotionalTag,
                    IsTrialEligible = price.IsTrialEligible,
                    TrialDays = price.TrialDays,

                    IsActive = product.IsActive && price.IsActive,
                    CreatedAt = product.CreatedAt
                }));
            }

            var sortedPlans = plans.OrderBy(p => p.DisplayOrder)
                                  .ThenBy(p => p.AmountInCents)
                                  .ToList();

            _cacheService.Add(cacheKey, sortedPlans, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {Count} subscription plans, activeOnly: {ActiveOnly}",
                sortedPlans.Count, activeOnly);

            return sortedPlans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all subscription plans, activeOnly: {ActiveOnly}", activeOnly);
            throw;
        }
    }

    #endregion

    #region Sync Operations

    public async Task SyncProductFromStripeAsync(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            _logger.LogInformation("Syncing product from Stripe: {ProductId}", productId);

            // TODO: Implement actual Stripe sync logic
            // This would involve calling the Stripe API to get the latest product data
            // and updating the local storage accordingly

            // For now, just update the sync timestamp
            var existingProduct = await GetProductAsync(productId);
            if (existingProduct != null)
            {
                existingProduct.LastSyncedFromStripe = DateTime.UtcNow;
                existingProduct.SyncStatus = "synced";
                await UpdateProductAsync(existingProduct);
            }

            _logger.LogInformation("Completed syncing product from Stripe: {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing product from Stripe: {ProductId}", productId);

            // Mark sync as failed
            var existingProduct = await GetProductAsync(productId);
            if (existingProduct == null) throw;
            existingProduct.SyncStatus = "error";
            await UpdateProductAsync(existingProduct);

            throw;
        }
    }

    public async Task SyncPriceFromStripeAsync(string productId, string priceId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(priceId))
            throw new ArgumentException("Price ID cannot be null or empty", nameof(priceId));

        try
        {
            _logger.LogInformation("Syncing price from Stripe: {PriceId} for product: {ProductId}",
                priceId, productId);

            // TODO: Implement actual Stripe sync logic
            // This would involve calling the Stripe API to get the latest price data
            // and updating the local storage accordingly

            // For now, just update the sync timestamp
            var existingPrice = await GetPriceAsync(productId, priceId);
            if (existingPrice != null)
            {
                existingPrice.LastSyncedFromStripe = DateTime.UtcNow;
                existingPrice.SyncStatus = "synced";
                await UpdatePriceAsync(existingPrice);
            }

            _logger.LogInformation("Completed syncing price from Stripe: {PriceId} for product: {ProductId}",
                priceId, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing price from Stripe: {PriceId} for product: {ProductId}",
                priceId, productId);

            // Mark sync as failed
            var existingPrice = await GetPriceAsync(productId, priceId);
            if (existingPrice != null)
            {
                existingPrice.SyncStatus = "error";
                await UpdatePriceAsync(existingPrice);
            }

            throw;
        }
    }

    public async Task SyncAllFromStripeAsync()
    {
        try
        {
            _logger.LogInformation("Starting sync of all products and prices from Stripe");

            var products = await GetAllProductsAsync(false); // Get all products including inactive

            var syncTasks = new List<Task>();

            foreach (var product in products)
            {
                // Sync product
                syncTasks.Add(SyncProductFromStripeAsync(product.StripeProductId));

                // Sync all prices for this product
                var prices = await GetProductPricesAsync(product.StripeProductId, false);
                syncTasks.AddRange(prices.Select(price =>
                    SyncPriceFromStripeAsync(product.StripeProductId, price.StripePriceId)));
            }

            await Task.WhenAll(syncTasks);

            _logger.LogInformation("Completed syncing all products and prices from Stripe. " +
                                 "Products: {ProductCount}, Total sync tasks: {TaskCount}",
                                 products.Count, syncTasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full sync from Stripe");
            throw;
        }
    }

    #endregion
}