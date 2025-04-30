using AutoMapper;
using DesolaDomain.Entities.AmadeusFields;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Entities.SkyScannerFields;

namespace DesolaInfrastructure.Mappers;

public class SkyScannerFlightMappingProfile : Profile
{
    public SkyScannerFlightMappingProfile()
    {
       CreateMap<SkySegment, FlightSegmentResponse>()
            .ForMember(dest => dest.FlightFrom, opt => opt.MapFrom(src => src.Origin.DisplayCode))
            .ForMember(dest => dest.FlightTo, opt => opt.MapFrom(src => src.Destination.DisplayCode))
            .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Departure))
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.Arrival))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => $"{src.MarketingCarrier.AlternateId} {src.FlightNumber}"))
            .ForMember(dest => dest.Airline, opt => opt.MapFrom(src => src.OperatingCarrier.Name))
           // .ForMember(dest => dest.AircraftPhotoLink, opt => opt.MapFrom(src => src.Carriers.Marketing.FirstOrDefault().LogoUrl))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.DurationInMinutes).ToString(@"hh\:mm")));


        CreateMap<FlightSegmentResponse, UnifiedSegment>()
            .ForMember(dest => dest.Departure, opt => opt.MapFrom(src => new UnifiedLocation
            {
                AirportCode = src.FlightFrom,
                DateTime = DateTime.Parse(src.DepartureDateTime),
                FormattedDateTime = FormatDateTime(DateTime.Parse(src.DepartureDateTime))
            }))
            .ForMember(dest => dest.Arrival, opt => opt.MapFrom(src => new UnifiedLocation
            {
                AirportCode = src.FlightTo,
                DateTime = DateTime.Parse(src.ArrivalDateTime),
                FormattedDateTime = FormatDateTime(DateTime.Parse(src.ArrivalDateTime))
            }))
            .ForMember(dest => dest.MarketingAirline, opt => opt.MapFrom(src => src.Airline))
            .ForMember(dest => dest.AirlineLogo, opt => opt.MapFrom(src => src.AircraftPhotoLink))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightNumber))
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => src.FlightDuration))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => ParseDuration(src.FlightDuration)))
            .ForMember(dest => dest.OperatingAirline, opt => opt.MapFrom(src => src.Airline)) // optional
            .ForMember(dest => dest.AircraftType, opt => opt.MapFrom(src => src.Aircraft))
            .ForMember(dest => dest.CabinClass, opt => opt.MapFrom(src => "Economy"));
    }
    
    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("MMM d, h:mm tt"); // Example: "Jan 4, 5:30 PM"
    }

    private static TimeSpan ParseDuration(string formattedDuration)
    {
        if (string.IsNullOrEmpty(formattedDuration)) return TimeSpan.Zero;

        var parts = formattedDuration.Split(':');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int hours) &&
            int.TryParse(parts[1], out int minutes))
        {
            return new TimeSpan(hours, minutes, 0);
        }

        return TimeSpan.Zero;
    }
}