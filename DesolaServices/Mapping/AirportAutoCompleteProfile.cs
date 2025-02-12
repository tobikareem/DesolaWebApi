using AutoMapper;
using DesolaDomain.Model;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class AirportAutoCompleteProfile : Profile
{
    public AirportAutoCompleteProfile()
    {
        CreateMap<AutocompleteData, AirportAutoCompleteResponse>()
            .ForMember(dest => dest.AirportId, opt => opt.MapFrom(src => src.Presentation.Id))
            .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.Navigation.EntityId))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Presentation.Subtitle))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Presentation.SuggestionTitle))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Navigation.LocalizedName))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Presentation.SkyId))
            .ForMember(dest => dest.AirportType, opt => opt.MapFrom(src => src.Navigation.EntityType));
    }

}