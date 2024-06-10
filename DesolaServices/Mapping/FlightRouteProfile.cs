using AutoMapper;
using DesolaDomain.Aggregates;
using DesolaDomain.Entities.Flights;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class FlightRouteProfile : Profile
{
    public FlightRouteProfile()
    {
        CreateMap<RouteLocation, FlightRouteResponse>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Subtype, opt => opt.MapFrom(src => src.Subtype))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.IataCode, opt => opt.MapFrom(src => src.IataCode))
            .ForMember(dest => dest.GeoCode, opt => opt.MapFrom(src => src.GeoCode))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone.ReferenceLocalDateTime));

        CreateMap<GeoCode, GeoCodeResponse>();
        CreateMap<Address, AddressResponse>();
    }
}