using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DesolaDomain.Settings;
using DesolaServices.Interfaces;
using DesolaServices.Services;

namespace DesolaServices;

public static class ServiceCollection
{
    public static IServiceCollection AddDesolaApplications(this IServiceCollection services, AppSettings appSettings)
    {

        services.AddScoped<IFlightSearchService, FlightSearchService>();
        services.AddScoped<IAirlineRouteService, AirlineRouteService>();
        services.AddScoped<IAirportScannerService, AirportScannerService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}