using AutoMapper;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.GoogleFields.Request;

namespace DesolaInfrastructure.Mappers;

public class GoogleFlightMappingProfile: Profile
{
    public GoogleFlightMappingProfile()
    {
        CreateMap<FlightSearchParameters, GoogleFlightRequest>()
            .ForMember(dest => dest.DepartureId, opt => opt.MapFrom(src => src.Destination))
            .ForMember(dest => dest.ArrivalId, opt => opt.MapFrom(src => src.Origin))
            .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.ArrivalDate,
                opt => opt.MapFrom(src => src.ReturnDate.HasValue ? src.ReturnDate.Value.ToString("yyyy-MM-dd") : null))
            .ForMember(dest => dest.CabinClass, opt => opt.MapFrom(src => MapCabinClass(src.CabinClass)))
            .ForMember(dest => dest.StopOver, opt => opt.MapFrom(src => src.NonStop ? 0: 1))
            .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src => src.MaxPrice))
            .ForMember(dest => dest.FlightSortOption, opt => opt.MapFrom(src => MapSortOption(src.SortBy)))
            .ForMember(dest => dest.Adults, opt => opt.MapFrom(src => src.Adults))
            .ForMember(dest => dest.IsOneWay, opt => opt.MapFrom(src => !src.ReturnDate.HasValue));

    }

    private static int MapSortOption(string srcSortBy)
    {
        if (string.IsNullOrWhiteSpace(srcSortBy))
            return (int)FlightSortOption.Price;

        return srcSortBy.Trim().ToLowerInvariant() switch
        {
            "top flights" => (int)FlightSortOption.TopFlights,
            "price" => (int)FlightSortOption.Price,
            "departure time" => (int)FlightSortOption.DepartureTime,
            "arrival time" => (int)FlightSortOption.ArrivalTime,
            "duration" => (int)FlightSortOption.Duration,
            "emissions" => (int)FlightSortOption.Emissions,
            _ => (int)FlightSortOption.Price
        };
    }

    private static int MapCabinClass(string srcCabinClass)
    {
        if (string.IsNullOrWhiteSpace(srcCabinClass))
            return (int)CabinClassOption.Economy;

        return srcCabinClass.Trim().ToUpperInvariant() switch
        {
            "ECONOMY" => (int)CabinClassOption.Economy,
            "PREMIUM ECONOMY" => (int)CabinClassOption.PremiumEconomy,
            "BUSINESS" => (int)CabinClassOption.Business,
            "FIRST" => (int)CabinClassOption.First,
            _ => (int)CabinClassOption.Economy
        };
    }

}