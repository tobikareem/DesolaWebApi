using OfficeOpenXml;
using DesolaDomain.Model;
using System.Collections.Concurrent;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Interface;
using Microsoft.Extensions.Configuration;

namespace DesolaDataSource.Repository;

public class AirportRepository : IAirportRepository
{
    private readonly IConfiguration _configuration;
    private readonly IBlobStorageRepository _blobStorageRepository;
    private readonly ICacheService _cacheService;
    public AirportRepository(IConfiguration configuration, IBlobStorageRepository blobStorageRepository, ICacheService cacheService)
    {
        _configuration = configuration;
        _blobStorageRepository = blobStorageRepository;
        _cacheService=cacheService;
    }
    public async Task<IEnumerable<Airport>> GetAirportsAsync()
    {
        return await Task.Run(GetAllAirports);
    }

    private async Task<List<Airport>> GetAllAirports()
    {
        if (_cacheService.Contains(CacheEntry.AllAirports))
        {
            return _cacheService.GetItem<List<Airport>>(CacheEntry.AllAirports) ?? new List<Airport>();
        }

        var fileName = _configuration["FileName"]?.ToLowerInvariant();
        var containerName = _configuration["ContainerName"]?.ToLowerInvariant();

        var stream =  await _blobStorageRepository.DownloadBlobAsStreamAsync(fileName, containerName);

        var airports = GetAirportsAsync(stream).ToList();

        _cacheService.Add(CacheEntry.AllAirports, airports, 600);

        return airports;
    }

    private static ConcurrentBag<Airport> GetAirportsAsync(Stream stream)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

        var rowCount = worksheet?.Dimension.Rows;


        var concurrentAirports = new ConcurrentBag<Airport>();
        if (rowCount == null)
        {
            return concurrentAirports;
        }

        Parallel.For(2, rowCount.Value + 1, row =>
        {
            if (worksheet == null)
            {
                return; // Skip if the worksheet is somehow null
            }

            var airportCode = worksheet.Cells[row, 14].Text; // Assuming IATA code is in the third column

            if (string.IsNullOrWhiteSpace(airportCode))
            {
                return; // Skip rows where the airport code is missing or blank
            }

            var airport = new Airport
            {
                Name = worksheet.Cells[row, 4].Text, // Name in the first column
                City = worksheet.Cells[row, 11].Text, // City in the second column
                Code = airportCode
            };

            concurrentAirports.Add(airport);
        });

        return concurrentAirports;
    }


}