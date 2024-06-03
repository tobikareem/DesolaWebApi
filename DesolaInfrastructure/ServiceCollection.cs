using CaptainOath.DataStore.Extension;
using DesolaDataSource.Repository;
using DesolaDomain.Interface;
using DesolaMemoryCache;
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
