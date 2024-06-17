using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Interfaces;

public interface IAirportScannerService
{
    Task<List<AirportAutoCompleteResponse>> GetAutocompleteResultsAsync(string query,
        CancellationToken cancellationToken);
}