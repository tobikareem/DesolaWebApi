using DesolaServices.Commands.Queries;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler
{
    public class SearchAirportAutoCompleteHandler : IRequestHandler<AirportAutoCompleteQuery, List<AirportAutoCompleteResponse>>
    {
        private readonly IAirportScannerService _requestHandlerImplementation;

        public SearchAirportAutoCompleteHandler(IAirportScannerService requestHandlerImplementation)
        {
            _requestHandlerImplementation = requestHandlerImplementation;
        }

        public async Task<List<AirportAutoCompleteResponse>> Handle(AirportAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            return await _requestHandlerImplementation.GetAutocompleteResultsAsync(request.AirportSearchQuery, cancellationToken);
        }
    }
}
