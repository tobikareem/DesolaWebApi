using System.Diagnostics;
using amadeus.resources;
using AutoMapper;
using DesolaDomain.Entities.FlightSearch;
using BaggageAllowance = DesolaDomain.Entities.FlightSearch.BaggageAllowance;

namespace DesolaInfrastructure.Mappers;

public class AmadeusFlightMappingProfile : Profile
{

    public AmadeusFlightMappingProfile()
    {
        CreateMap<List<FlightOffer>, UnifiedFlightSearchResponse>()
            .ForMember(dest => dest.TotalResults, opt => opt.MapFrom(src => src.Count))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.FirstOrDefault().price.currency ?? "USD"))
            .ForMember(dest => dest.Offers, opt => opt.MapFrom(src => src));

        CreateMap<FlightOffer, UnifiedFlightOffer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => "Amadeus"))
            .ForMember(dest => dest.FlightSource, opt => opt.MapFrom(src => src.source))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => decimal.Parse(src.price.total)))
            .ForMember(dest => dest.FormattedPrice, opt => opt.MapFrom(src => $"{src.price.currency} {src.price.total}"))
            .ForMember(dest => dest.Itineraries, opt => opt.MapFrom(src => MapItineraries(src.itineraries)))
            .ForMember(dest => dest.BaggageAllowance, opt => opt.MapFrom(src => MapBaggageAllowance(src.travelerPricings)))
            .ForMember(dest => dest.IsRefundable, opt => opt.MapFrom(src => src.pricingOptions.refundableFare))
            .ForMember(dest => dest.LastTicketingDate, opt => opt.MapFrom(src =>
               DateTime.Parse(src.lastTicketingDate)))
            
            .ForMember(dest => dest.ValidatingCarrier, opt => opt.MapFrom((src, dest, _, context) => GetAirlineName<FlightOffer>(src, context, "ValidatingCarrier")))
            
            .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.numberOfBookableSeats))
            .ForMember(dest => dest.FareConditions, opt => opt.MapFrom(src => ExtractFareConditions(src)));

        CreateMap<Segment, UnifiedSegment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
            .ForMember(dest => dest.Departure, opt => opt.MapFrom(src => src.departure))
            .ForMember(dest => dest.Arrival, opt => opt.MapFrom(src => src.arrival))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => ParseDuration(src.duration)))
            .ForMember(dest => dest.FormattedDuration, opt => opt.MapFrom(src => FormatDuration(ParseDuration(src.duration))))
            
            .ForMember(dest => dest.MarketingAirline, opt => opt.MapFrom((src, dest, _, context) => GetAirlineName<Segment>(src, context, "MarketingAirline")))
            .ForMember(dest => dest.OperatingAirline, opt => opt.MapFrom((src, dest, _, context) => GetAirlineName<Segment>(src, context, "OperatingAirline")))

            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.number))
            .ForMember(dest => dest.AircraftType, opt => opt.MapFrom(src => src.aircraft.code))
            .ForMember(dest => dest.CabinClass, opt => opt.MapFrom((src, dest, _, context) =>
                GetCabinClass(src.id, context.Items["travelerPricings"] as List<TravelerPricing>)));

        CreateMap<FlightEndPoint, UnifiedLocation>()
            .ForMember(dest => dest.AirportCode, opt => opt.MapFrom(src => src.iataCode))
            .ForMember(dest => dest.Terminal, opt => opt.MapFrom(src => src.terminal))
            .ForMember(dest => dest.DateTime, opt => opt.MapFrom(src => DateTime.Parse(src.at)))
            .ForMember(dest => dest.FormattedDateTime, opt => opt.MapFrom(src => FormatDateTime(DateTime.Parse(src.at))));
    }


    private static string GetAirlineName<T>(T flightOffer, ResolutionContext context, string field)
    {
        // Use pattern matching on the flightOffer instance
        var airlineCode = flightOffer switch
        {
            FlightOffer fo => fo.validatingAirlineCodes?.FirstOrDefault() ?? string.Empty,
            Segment segment => field switch
            {
                "MarketingAirline" => segment.carrierCode,
                "OperatingAirline" => segment.operating?.carrierCode ?? segment.carrierCode,
                _ => string.Empty
            },
            _ => string.Empty
        };

        // Retrieve the list of airlines from the context and lookup by IATA code
        if (!context.Items.ContainsKey("Airlines"))
        {
            throw new InvalidOperationException("The 'Airlines' key is missing from the resolution context items.");
        }
        var airlineName = airlines?.FirstOrDefault(a => a.IataCode == airlineCode)?.Name ?? "Unknown Airline";

        return $"{airlineCode} - {airlineName}";
    }

    private List<UnifiedItinerary> MapItineraries(IReadOnlyList<Itineraries> itineraries)
    {
        var result = new List<UnifiedItinerary>();

        for (var i = 0; i < itineraries.Count; i++)
        {
            var itinerary = itineraries[i];
            var direction = i == 0 ? "Outbound" : "Return";

            result.Add(new UnifiedItinerary
            {
                Direction = direction,
                Duration = ParseDuration(itinerary.duration),
                FormattedDuration = FormatDuration(ParseDuration(itinerary.duration)),
                Stops = itinerary.segments.Count - 1,
                Segments = MapSegments(itinerary.segments)
            });
        }

        return result;
    }

    private IEnumerable<UnifiedSegment> MapSegments(IEnumerable<Segment> segments)
    {
        return segments.Select(segment => new UnifiedSegment
        {
            Id = segment.id,
            Departure = MapLocation(segment.departure),
            Arrival = MapLocation(segment.arrival),
            Duration = ParseDuration(segment.duration),
            FormattedDuration = FormatDuration(ParseDuration(segment.duration)),
            MarketingAirline = segment.carrierCode,
            OperatingAirline = segment.operating?.carrierCode ?? segment.carrierCode,
            FlightNumber = segment.number,
            AircraftType = segment.aircraft?.code,
            // CabinClass will be set elsewhere with context information
            BaggageAllowance = new BaggageAllowance() // Default empty, will be populated later
        });
    }

    private UnifiedLocation MapLocation(FlightEndPoint endPoint)
    {
        if (endPoint == null)
            return null;

        return new UnifiedLocation
        {
            AirportCode = endPoint.iataCode,
            Terminal = endPoint.terminal,
            // CityCode and CountryCode will be populated later from dictionaries
            DateTime = DateTime.Parse(endPoint.at),
            FormattedDateTime = FormatDateTime(DateTime.Parse(endPoint.at))
        };
    }

    private BaggageAllowance MapBaggageAllowance(IEnumerable<TravelerPricing> travelerPricing)
    {
        // Extract baggage allowance from the first adult traveler
        var adultTraveler = travelerPricing?.FirstOrDefault(tp => tp.travelerType == "ADULT");
        if (adultTraveler == null || adultTraveler.fareDetailsBySegment == null || !adultTraveler.fareDetailsBySegment.Any())
            return new BaggageAllowance();

        var firstSegment = adultTraveler.fareDetailsBySegment.First();
        var baggage = firstSegment.includedCheckedBags;

        return new BaggageAllowance
        {
            CheckedBags = baggage?.quantity ?? 0,
            WeightKg = baggage?.weight,
            Description = FormatBaggageDescription(new BaggageAllowance { CheckedBags = baggage?.quantity ?? 0, WeightKg = baggage?.weight })
        };
    }

    private string FormatBaggageDescription(BaggageAllowance baggage)
    {
        if (baggage == null)
            return "No checked baggage included";

        if (baggage.CheckedBags > 0)
            return $"{baggage.CheckedBags} bag(s)" + (baggage.WeightKg.HasValue ? $" up to {baggage.WeightKg}kg each" : "");

        return baggage.WeightKg.HasValue ? $"Up to {baggage.WeightKg}kg total" : "No checked baggage included";
    }

    private static TimeSpan ParseDuration(string duration)
    {
        // Parse ISO 8601 duration format (PT9H20M) to TimeSpan
        try
        {
            // Remove PT prefix
            var durationString = duration.Substring(2);

            int hours = 0;
            int minutes = 0;

            var hourIndex = durationString.IndexOf("H", StringComparison.Ordinal);
            if (hourIndex > 0)
            {
                hours = int.Parse(durationString.Substring(0, hourIndex));
                durationString = durationString.Substring(hourIndex + 1);
            }

            var minuteIndex = durationString.IndexOf("M", StringComparison.Ordinal);
            if (minuteIndex > 0)
            {
                minutes = int.Parse(durationString.Substring(0, minuteIndex));
            }

            return new TimeSpan(hours, minutes, 0);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }

    private string FormatDuration(TimeSpan duration)
    {
        return $"{(int)duration.TotalHours}h {duration.Minutes}m";
    }

    private string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("MMM d, h:mm tt");
    }

    private string GetCabinClass(string segmentId, List<TravelerPricing> travelerPricings)
    {
        if (travelerPricings == null || !travelerPricings.Any())
            return "Economy";

        foreach (var traveler in travelerPricings)
        {
            var segment = traveler.fareDetailsBySegment?.FirstOrDefault(s => s.segmentId == segmentId);
            if (segment != null)
                return segment.cabin;
        }

        return "Economy";
    }

    private List<string> ExtractFareConditions(FlightOffer offer)
    {
        var conditions = new List<string>();

        if (offer.pricingOptions?.refundableFare == true)
            conditions.Add("Refundable");
        else
            conditions.Add("Non-refundable");

        if (offer.pricingOptions?.includedCheckedBagsOnly == true)
            conditions.Add("Checked baggage included");
        else
            conditions.Add("No checked baggage included");

        if (offer.pricingOptions?.noRestrictionFare == true)
            conditions.Add("No restrictions");

        if (offer.pricingOptions?.noPenaltyFare == true)
            conditions.Add("No change fees");

        return conditions;
    }
}