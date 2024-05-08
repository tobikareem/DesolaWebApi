using OfficeOpenXml;
using DesolaDomain.Model;
using System.Collections.Concurrent;
using CaptainOath.DataStore.Interface;
using DesolaDomain.Interface;
using Microsoft.Extensions.Configuration;

namespace DesolaDataSource.Repository;

public class AirportRepository(IBlobStorageRepository blobStorageRepository, IConfiguration configuration) : IAirportRepository
{
    public async Task<IEnumerable<Airport>> GetAirportsAsync()
    {
        return await Task.Run(GetAllAirports);
    }

    private async Task<List<Airport>> GetAllAirports()
    {
        var fileName = configuration["FileName"];
        var containerName = configuration["ContainerName"];
        var stream = await blobStorageRepository.DownloadBlobAsStreamAsync(fileName, containerName);

        var airport = await Task.Run(() => GetAirportsAsync(stream));

        return airport.ToList();
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