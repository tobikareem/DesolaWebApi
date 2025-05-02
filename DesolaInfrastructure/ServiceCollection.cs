using amadeus;
using Azure.Data.Tables;
using CaptainOath.DataStore.Extension;
using CaptainOath.DataStore.Interface;
using CaptainOath.DataStore.Repositories;
using DesolaDomain.Entities.Pages;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using DesolaInfrastructure.Data;
using DesolaInfrastructure.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using DesolaInfrastructure.External.Providers.Amadeus;
using DesolaInfrastructure.External.Providers.SkyScanner;

namespace DesolaInfrastructure;

public static class ServiceCollection
{
    public static IServiceCollection AddDesolaInfrastructure(this IServiceCollection services, AppSettings configuration)
    {
        services.AddSingleton<IAirportRepository, AirportRepository>();
        services.AddSingleton<IAirlineRepository, AirlineRepository>();
        services.AddSingleton<ICacheService, CacheService>();

        var connectionString = configuration.BlobFiles.BlobUri;

        var storageAccountConnectionString = configuration.StorageAccount.ConnectionString;

        if (string.IsNullOrWhiteSpace(storageAccountConnectionString))
        {
            throw new ArgumentNullException(nameof(storageAccountConnectionString));
        }

        services.AddBlobClientUri(connectionString);
        services.AddBlobStorageClientUsingConnectionString(storageAccountConnectionString);
        services.AddTableStorageClientCheck(storageAccountConnectionString);

        services.AddHttpClient();
        services.AddScoped<IHttpService, HttpService>();
        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IAmadeusService, AmadeusService>();

        services.AddAutoMapper(Assembly.GetExecutingAssembly());


        services.AddScoped<AmadeusFlightProvider>();
        services.AddScoped<SkyScannerFlightProvider>();

        services.AddScoped<IEnumerable<IFlightProvider>>(serviceProvider => new List<IFlightProvider>
        {
            serviceProvider.GetRequiredService<AmadeusFlightProvider>(),
            serviceProvider.GetRequiredService<SkyScannerFlightProvider>()
        });

        //var amadeus = Amadeus
        //    .builder(configuration.ExternalApi.Amadeus.ClientId, configuration.ExternalApi.Amadeus.ClientSecret)
        //    .build();
        //services.AddSingleton(amadeus);


        return services;
    }

    public static IServiceCollection AddTableStorageClientCheck(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<ITableStorageRepository<WebSection>, TableStorageRepository<WebSection>>();
        services.AddSingleton<ITableStorageRepository<UserTravelPreference>, TableStorageRepository<UserTravelPreference>>();

        services.AddSingleton(_ =>
        {
            var tableServiceClient = new TableServiceClient(connectionString);
            return tableServiceClient;
        });

        return services;
    }
}