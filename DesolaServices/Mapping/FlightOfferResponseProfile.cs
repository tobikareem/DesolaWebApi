using AutoMapper;
using Desola.Common;
using DesolaDomain.Aggregates;
using DesolaDomain.Entities.Flights;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class FlightOfferResponseProfile : Profile
{

    public FlightOfferResponseProfile()
    {
        CreateMap<Segment, FlightSegmentResponse>()
            .ForMember(dest => dest.FlightFrom, opt => opt.MapFrom(src => src.Departure.IataCode))
            .ForMember(dest => dest.FlightTo, opt => opt.MapFrom(src => src.Arrival.IataCode))
            .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Departure.At.ToDateTime()))
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.Arrival.At.ToDateTime()))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Number))
            .ForMember(dest => dest.Airline, opt => opt.MapFrom(src => src.CarrierCode))
            .ForMember(dest => dest.Aircraft, opt => opt.MapFrom(src => src.Aircraft.Code))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => src.Duration));


        CreateMap<Itinerary, FlightItineraryResponse>()
            .ForMember(dest => dest.NumberOfStopOver, opt => opt.MapFrom(src => src.Segments.Count - 1))
            .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => src.Duration))
            .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Segments));

        CreateMap<TravelerPricing, FlightTravelerPricingResponse>()
            .ForMember(dest => dest.TravelerId, opt => opt.MapFrom(src => src.TravelerId))
            .ForMember(dest => dest.FareOption, opt => opt.MapFrom(src => src.FareOption))
            .ForMember(dest => dest.TravelerType, opt => opt.MapFrom(src => src.TravelerType))
            .ForMember(dest => dest.PriceCurrency, opt => opt.MapFrom(src => src.Price.Currency))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price.Total));

    }
}