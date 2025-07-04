using DesolaDomain.Entities.Payment;

namespace DesolaServices.Interfaces;

public interface IDesolaProductAndPriceStorage
{
    // Product Operations
    Task<DesolaProductDetail> CreateProductAsync(DesolaProductDetail product);
    Task<DesolaProductDetail> GetProductAsync(string productId);
    Task<DesolaProductDetail> UpdateProductAsync(DesolaProductDetail product);
    Task<List<DesolaProductDetail>> GetAllProductsAsync(bool activeOnly = true);
    Task<bool> DeleteProductAsync(string productId);

    // Price Operations  
    Task<DesolaPriceDetail> CreatePriceAsync(DesolaPriceDetail price);
    Task<DesolaPriceDetail> GetPriceAsync(string productId, string priceId);
    Task<DesolaPriceDetail> UpdatePriceAsync(DesolaPriceDetail price);
    Task<List<DesolaPriceDetail>> GetProductPricesAsync(string productId, bool activeOnly = true);
    Task<List<DesolaPriceDetail>> GetAllPricesAsync(bool activeOnly = true);
    Task<bool> DeletePriceAsync(string productId, string priceId);

    // Combined Operations
    Task<DesolaSubscriptionPlan> GetSubscriptionPlanAsync(string productId, string priceId);
    Task<List<DesolaSubscriptionPlan>> GetAllSubscriptionPlansAsync(bool activeOnly = true);

    // Sync Operations
    Task SyncProductFromStripeAsync(string productId);
    Task SyncPriceFromStripeAsync(string productId, string priceId);
    Task SyncAllFromStripeAsync();
}