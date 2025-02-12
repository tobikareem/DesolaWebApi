using amadeus;
using CaptainOath.DataStore.Extension;
using DesolaDomain.Interfaces;
using DesolaInfrastructure.Data;
using DesolaInfrastructure.External;
using DesolaInfrastructure.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.AddBlobClientUri(connectionString);
            services.AddBlobStorageClientUsingConnectionString(storageAccountConnectionString);

            services.AddScoped<IAmadeusService, AmadeusService>();

            services.AddHttpClient();
            services.AddScoped<IHttpService, HttpService>();
            services.AddScoped<IApiService, ApiService>();

            var amadeus = Amadeus
                .builder(configuration["Amadeus_client_id"], configuration["Amadeus_client_secret"])
                .build();
            services.AddSingleton(amadeus);


            return services;
        }


    }


}
