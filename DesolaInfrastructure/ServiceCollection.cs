using amadeus;
using Azure.Data.Tables;
using CaptainOath.DataStore.Extension;
using CaptainOath.DataStore.Interface;
using CaptainOath.DataStore.Repositories;
using DesolaDomain.Entities.PageEntity;
using DesolaDomain.Interfaces;
using DesolaInfrastructure.Data;
using DesolaInfrastructure.External;
using DesolaInfrastructure.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DesolaInfrastructure
{
    public static class ServiceCollection
    {

        public static IServiceCollection AddDesolaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IAirportRepository, AirportRepository>();
            services.AddSingleton<IAirlineRepository, AirlineRepository>();
            services.AddSingleton<ICacheService, CacheService>();
            
            var connectionString = configuration["BlobUri"];

            var storageAccountConnectionString = configuration["AzureStorageAccountConnectionString"];

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

            var amadeus = Amadeus
                .builder(configuration["Amadeus_client_id"], configuration["Amadeus_client_secret"])
                .build();
            services.AddSingleton(amadeus);


            return services;
        }

        public static IServiceCollection AddTableStorageClientCheck(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<ITableStorageRepository<WebSection>, TableStorageRepository<WebSection>>();

            services.AddSingleton(_ =>
            {
                var tableServiceClient = new TableServiceClient(connectionString);
                return tableServiceClient;
            });

            return services;
        }
    }


}
