using AutoMapper;
using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class FlightOfferResponseProfile: Profile
{

    public FlightOfferResponseProfile()
    {
        CreateMap<FlightOffer, FlightSearchResponse>()
            .ForMember(dest => dest.FlightFrom, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).FirstOrDefault().Departure.IataCode))
            .ForMember(dest => dest.FlightTo, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).LastOrDefault().Arrival.IataCode))
            .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).FirstOrDefault().Departure.At))
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).LastOrDefault().Arrival.At))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).FirstOrDefault().Number))
            .ForMember(dest => dest.Airline, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).FirstOrDefault().CarrierCode))
            .ForMember(dest => dest.Aircraft, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).FirstOrDefault().Aircraft.Code))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).FirstOrDefault().Duration))
            .ForMember(dest => dest.NumberOfStopOver, opt => opt.MapFrom(src => src.Data.SelectMany(d => d.Itineraries).SelectMany(i => i.Segments).Count() - 1))
            .ForMember(dest => dest.PriceCurrency, opt => opt.MapFrom(src => src.Data.FirstOrDefault().Price.Currency))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Data.FirstOrDefault().Price.GrandTotal));
    }
}