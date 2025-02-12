using AutoMapper;
using DesolaDomain.Aggregates;
using DesolaDomain.Entities.Flights;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class SkyScannerFlightOfferProfile : Profile
{
    public SkyScannerFlightOfferProfile()
    {
        CreateMap<SkySegment, FlightSegmentResponse>()
            .ForMember(dest => dest.FlightFrom, opt => opt.MapFrom(src => src.Origin.DisplayCode))
            .ForMember(dest => dest.FlightTo, opt => opt.MapFrom(src => src.Destination.DisplayCode))
            .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Departure))
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.Arrival))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => $"{src.MarketingCarrier.AlternateId} {src.FlightNumber}"))
            .ForMember(dest => dest.Airline, opt => opt.MapFrom(src => src.OperatingCarrier.Name))
            //.ForMember(dest => dest.AircraftPhotoLink, opt => opt.MapFrom(src => src.Carriers.Marketing.FirstOrDefault().LogoUrl))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.DurationInMinutes).ToString(@"hh\:mm")));

    }
}