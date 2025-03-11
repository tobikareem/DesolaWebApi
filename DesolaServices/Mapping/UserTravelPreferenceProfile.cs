using AutoMapper;
using DesolaDomain.Entities.User;
using DesolaServices.DataTransferObjects.Requests;

namespace DesolaServices.Mapping;

public class UserTravelPreferenceProfile: Profile
{
    public UserTravelPreferenceProfile()
    {
        CreateMap<UserTravelPreferenceRequest, UserTravelPreference>()
            .ForMember(dest => dest.OriginAirport, opt => opt.MapFrom(src => src.OriginAirport))
            .ForMember(dest => dest.DestinationAirport, opt => opt.MapFrom(src => src.DestinationAirport))
            .ForMember(dest => dest.TravelClass, opt => opt.MapFrom(src => src.TravelClass))
            .ForMember(dest => dest.StopOvers, opt => opt.MapFrom(src => src.StopOvers))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
    }
}