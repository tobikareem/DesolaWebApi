using System.Text.Json;
using AutoMapper;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Entities.User;
using DesolaServices.DataTransferObjects.Requests;
using Newtonsoft.Json;

namespace DesolaServices.Mapping;

public class UserClickTrackingProfile: Profile
{
    public UserClickTrackingProfile()
    {
        CreateMap<ClickTrackingPayload, UserClickTracking>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.FlightOffer, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.UnifiedFlightOffer)))
            .ForMember(dest => dest.ClickedAt, opt => opt.MapFrom(src => src.ClickedAt));


        CreateMap<UserClickTracking, ClickTrackingPayload>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UnifiedFlightOffer, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<UnifiedFlightOffer>(src.FlightOffer)))
            .ForMember(dest => dest.ClickedAt, opt => opt.MapFrom(src => src.ClickedAt));

    }
}