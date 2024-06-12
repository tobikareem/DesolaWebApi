using AutoMapper;
using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class SkyScannerFlightOfferProfile : Profile
{
    public SkyScannerFlightOfferProfile()
    {
        CreateMap<SkyScannerLeg, FlightSegmentResponse>()
            .ForMember(dest => dest.FlightFrom, opt => opt.MapFrom(src => src.Origin.DisplayCode))
            .ForMember(dest => dest.FlightTo, opt => opt.MapFrom(src => src.Destination.DisplayCode))
            .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Departure))
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.Arrival))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Airline, opt => opt.MapFrom(src => src.Carriers.Marketing.FirstOrDefault().Name))
            .ForMember(dest => dest.Aircraft, opt => opt.MapFrom(src => src.Carriers.Marketing.FirstOrDefault().LogoUrl))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.DurationInMinutes).ToString(@"hh\:mm")));

        CreateMap<SkyScannerItinerary, FlightItineraryResponse>()
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price.Raw))
            .ForMember(dest => dest.PriceCurrency, opt => opt.MapFrom(src => "USD"))
            .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Legs));
    }
}