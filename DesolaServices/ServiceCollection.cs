using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaServices.Interfaces;
using DesolaServices.Services;
using CaptainPayment.Extensions;

namespace DesolaServices;

public static class ServiceCollection
{
    public static IServiceCollection AddDesolaApplications(this IServiceCollection services, AppSettings configuration)
    {

        services.AddScoped<IFlightSearchService, FlightSearchService>();
        services.AddScoped<IAirlineRouteService, AirlineRouteService>();
        services.AddScoped<IAirportScannerService, AirportScannerService>();
        services.AddScoped<ICustomerManagementService, CustomerManagementService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddStripePayments(options =>
        {
            options.SecretKey = configuration.Payment.Stripe.SandboxClientSecret; // This is usually the secret key for test mode
            options.PublishableKey = configuration.Payment.Stripe.SandboxClientId; // This is usually the publishable key
            options.WebhookSecret = configuration.Payment.Stripe.WebhookSecret;
            options.ProviderName = configuration.Payment.Stripe.ProviderName;
            options.PaymentOptions = configuration.Payment.Stripe.PaymentOptions;
            options.SubscriptionDefaults = configuration.Payment.Stripe.SubscriptionDefaults;
        });

        return services;
    }
}