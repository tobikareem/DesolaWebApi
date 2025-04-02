using AutoMapper;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Queries.Airports;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Handler.Airports
{
    public class GetAirportAutoCompleteQueryHandler : IRequestHandler<GetAirportAutoCompleteQuery, List<AirportBasicResponse>>
    {
        private readonly IAirportRepository _airportRepository;
        private readonly IMapper _mapper;

        public GetAirportAutoCompleteQueryHandler(IAirportRepository airportRepository, IMapper mapper)
        {
            _airportRepository = airportRepository;
            _mapper = mapper;
        }

        public async Task<List<AirportBasicResponse>> Handle(GetAirportAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var airportList = await _airportRepository.SearchAirportsAsync(request.AirportSearchQuery);

            var response = _mapper.Map<List<AirportBasicResponse>>(airportList);

            return response;
        }
    }
}
