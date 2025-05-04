using AutoMapper;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.SkyScannerFields;
using System.Reflection;
using System.Runtime.Serialization;

namespace DesolaInfrastructure.Mappers;

public class FlightSearchParametersSkyScannerFlightRequest: Profile
{
    public FlightSearchParametersSkyScannerFlightRequest()
    {
        CreateMap<FlightSearchParameters, SkyScannerFlightRequest>()
            .ForMember(dest => dest.FromEntityId, opt => opt.MapFrom(src => src.Origin))
            .ForMember(dest => dest.ToEntityId, opt => opt.MapFrom(src => src.Destination))
            .ForMember(dest => dest.DepartDate, opt => opt.MapFrom(src => src.DepartureDate.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.ReturnDate, opt => opt.MapFrom(src =>
                src.ReturnDate.HasValue ? src.ReturnDate.Value.ToString("yyyy-MM-dd") : null))
            .ForMember(dest => dest.Adults, opt => opt.MapFrom(src => src.Adults))
            .ForMember(dest => dest.Infants, opt => opt.MapFrom(src => src.Infants))
            .ForMember(dest => dest.CabinClass, opt => opt.MapFrom(src => MapCabinClass(src.CabinClass)))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.CurrencyCode))
            .ForMember(dest => dest.Stops, opt => opt.MapFrom(src => src.NonStop ? "direct" : "direct,1stop"))
            .ForMember(dest => dest.SortBy, opt => opt.MapFrom(src => src.SortBy))
            .ForMember(dest => dest.SortOrder, opt => opt.MapFrom(src => src.SortOrder))
            .ForMember(dest => dest.IsOneWay, opt => opt.MapFrom(src => !src.ReturnDate.HasValue));
    }

    private static string MapCabinClass(string amadeusClass)
    {
        if (string.IsNullOrWhiteSpace(amadeusClass))
            return GetEnumMemberValue(CabinClassOption.Economy);

        var match = amadeusClass.Trim().ToLowerInvariant() switch
        {
            "economy" => CabinClassOption.Economy,
            "premium_economy" => CabinClassOption.PremiumEconomy,
            "business" => CabinClassOption.Business,
            "first" => CabinClassOption.First,
            _ => CabinClassOption.Economy
        };

        return GetEnumMemberValue(match);
    }

    private static string GetEnumMemberValue(Enum enumValue)
    {
        var type = enumValue.GetType();
        var memberInfo = type.GetMember(enumValue.ToString()).FirstOrDefault();
        var enumMemberAttr = memberInfo?.GetCustomAttribute<EnumMemberAttribute>();
        return enumMemberAttr?.Value ?? enumValue.ToString().ToLowerInvariant();
    }
}