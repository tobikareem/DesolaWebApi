using AutoMapper;
using DesolaDomain.Model;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class AirportBasicProfile : Profile
{
    public AirportBasicProfile()
    {
        CreateMap<Airport, AirportBasicResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code));
    }
}