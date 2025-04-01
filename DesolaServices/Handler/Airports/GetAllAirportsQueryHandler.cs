using AutoMapper;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Queries.Airports;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Airports;

public class GetAllAirportsQueryHandler : IRequestHandler<GetAllAirportsQuery, List<AirportBasicResponse>>
{
    private readonly IAirportRepository _airportRepository;
    private readonly ILogger<GetAllAirportsQueryHandler> _logger;
    private readonly IMapper _mapper;
    public GetAllAirportsQueryHandler(IAirportRepository airportRepository, ILogger<GetAllAirportsQueryHandler> logger, IMapper mapper)
    {
        _airportRepository = airportRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<List<AirportBasicResponse>> Handle(GetAllAirportsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all airports from repository");
        var airportList = await _airportRepository.GetAirportsAsync();

        var response = _mapper.Map<List<AirportBasicResponse>>(airportList);

        return response;
    }
}