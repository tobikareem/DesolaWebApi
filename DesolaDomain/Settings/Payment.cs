using CaptainPayment.Core.Config;

namespace DesolaDomain.Settings;

public class Payment
{
    public Stripe Stripe { get; set; } = new();
}

public class Stripe
{
    public string ProviderName { get; set; } = "Stripe";
    public string AuthenticatorEmergencyCode { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string SandboxClientId { get; set; } = string.Empty;
    public string SandboxClientSecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    public StripePaymentOptions PaymentOptions { get; set; } = new();
    public StripeSubscriptionDefaults SubscriptionDefaults { get; set; } = new();


}