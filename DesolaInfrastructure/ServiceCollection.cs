using CaptainOath.DataStore.Extension;
using DesolaDomain.Interfaces;
using DesolaInfrastructure.Data;
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

            services.AddSingleton<ICacheService, CacheService>();

            var connectionString = configuration["BlobUri"];

            services.AddBlobClientUri(connectionString);
            return services;
        }


    }


}
