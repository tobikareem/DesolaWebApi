using AutoMapper;
using DesolaDomain.Aggregates;
using DesolaDomain.Entities.AmadeusFields;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Entities.SkyScannerFields;

namespace DesolaInfrastructure.Mappers;

public class SkyScannerFlightMappingProfile : Profile
{
    public SkyScannerFlightMappingProfile()
    {
        // Map SkyScanner segment to intermediate flight segment response
        CreateMap<SkySegment, FlightSegmentResponse>()
            .ForMember(dest => dest.FlightFrom, opt => opt.MapFrom(src => src.Origin.DisplayCode))
            .ForMember(dest => dest.FlightTo, opt => opt.MapFrom(src => src.Destination.DisplayCode))
            .ForMember(dest => dest.DepartureDateTime, opt => opt.MapFrom(src => src.Departure))
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.Arrival))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => $"{src.MarketingCarrier.AlternateId} {src.FlightNumber}"))
            .ForMember(dest => dest.Airline, opt => opt.MapFrom(src => src.OperatingCarrier.Name))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => TimeSpan.FromMinutes(src.DurationInMinutes).ToString(@"hh\:mm")));

        // Map intermediate flight segment to unified segment
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
            .ForMember(dest => dest.OperatingAirline, opt => opt.MapFrom(src => src.Airline))
            .ForMember(dest => dest.AircraftType, opt => opt.MapFrom(src => src.Aircraft))
            .ForMember(dest => dest.CabinClass, opt => opt.MapFrom(src => "Economy"));

        // Direct map from SkyScanner response to unified response
        CreateMap<SkyScannerFlightOffer, UnifiedFlightSearchResponse>()
            .ForMember(dest => dest.TotalResults, opt => opt.MapFrom(src => src.Data.Itineraries.Count))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => "USD"))
            .ForMember(dest => dest.Offers, opt => opt.MapFrom((src, dest, _, context) =>
                MapOffers(src, context.Items.ContainsKey("AirlineLogos") ? context.Items["AirlineLogos"] as Dictionary<string, string> : null)))
            .ForMember(dest => dest.Origin, opt => opt.MapFrom((src, _, __, context) =>
                context.Items.ContainsKey("Parameters") ? ((FlightSearchParameters)context.Items["Parameters"]).Origin : null))
            .ForMember(dest => dest.Destination, opt => opt.MapFrom((src, _, __, context) =>
                context.Items.ContainsKey("Parameters") ? ((FlightSearchParameters)context.Items["Parameters"]).Destination : null))
            .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom((src, _, __, context) =>
                context.Items.ContainsKey("Parameters") ? ((FlightSearchParameters)context.Items["Parameters"]).DepartureDate : DateTime.Now))
            .ForMember(dest => dest.ReturnDate, opt => opt.MapFrom((src, _, __, context) =>
                context.Items.ContainsKey("Parameters") ? ((FlightSearchParameters)context.Items["Parameters"]).ReturnDate : null))
            .ForMember(dest => dest.Airlines, opt => opt.MapFrom(src => ExtractAirlines(src)))
            .ForMember(dest => dest.Airports, opt => opt.MapFrom(src => ExtractAirports(src)))
            .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => ExtractLocations(src)));
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

    private IEnumerable<UnifiedFlightOffer> MapOffers(SkyScannerFlightOffer source, Dictionary<string, string> airlineLogos)
    {
        var offers = new List<UnifiedFlightOffer>();

        foreach (var itinerary in source.Data.Itineraries)
        {
            // Extract itinerary IDs
            var itineraryIds = itinerary.Id.Split('|', StringSplitOptions.RemoveEmptyEntries);

            // Find matching legs
            var legs = itinerary.Legs.ToList();
            var departureLeg = legs.FirstOrDefault(l => l.Id == itineraryIds.FirstOrDefault());
            var returnLeg = itineraryIds.Length > 1 ? legs.FirstOrDefault(l => l.Id == itineraryIds[1]) : null;

            // Map to unified offer
            offers.Add(new UnifiedFlightOffer
            {
                Id = itinerary.Id,
                Provider = "SkyScanner",
                FlightSource = "SkyScanner",
                TotalPrice = itinerary.Price.Raw,
                FormattedPrice = $"USD {itinerary.Price.Raw}",
                Itineraries = BuildItineraries(departureLeg, returnLeg, airlineLogos),
                IsRefundable = false,
                BaggageAllowance = new BaggageAllowance
                {
                    CheckedBags = 0,
                    WeightKg = null,
                    Description = "No checked baggage information available"
                },
                FareConditions = new List<string> { "Fare conditions not specified" },
                AvailableSeats = 0,
                LastTicketingDate = null,
                ValidatingCarrier = departureLeg?.Segments.FirstOrDefault()?.MarketingCarrier?.AlternateId
            });
        }

        return offers;
    }

    private IEnumerable<UnifiedItinerary> BuildItineraries(SkyScannerLeg departureLeg, SkyScannerLeg returnLeg, Dictionary<string, string> airlineLogos)
    {
        var itineraries = new List<UnifiedItinerary>();

        if (departureLeg != null)
        {
            itineraries.Add(BuildItinerary("Outbound", departureLeg, airlineLogos));
        }

        if (returnLeg != null)
        {
            itineraries.Add(BuildItinerary("Return", returnLeg, airlineLogos));
        }

        return itineraries;
    }

    private UnifiedItinerary BuildItinerary(string direction, SkyScannerLeg leg, Dictionary<string, string> airlineLogos)
    {
        return new UnifiedItinerary
        {
            Direction = direction,
            Duration = TimeSpan.FromMinutes(leg.DurationInMinutes),
            FormattedDuration = TimeSpan.FromMinutes(leg.DurationInMinutes).ToString(@"hh\:mm"),
            Stops = leg.StopCount,
            Segments = leg.Segments.Select(s => BuildSegment(s, airlineLogos)).ToList()
        };
    }

    private static UnifiedSegment BuildSegment(SkySegment segment, IReadOnlyDictionary<string, string> airlineLogos)
    {
        var airlineCode = segment.MarketingCarrier?.AlternateId;
        string airlineLogo = null;

        if (!string.IsNullOrEmpty(airlineCode) && airlineLogos != null)
        {
            airlineLogos.TryGetValue(airlineCode, out airlineLogo);
        }

        return new UnifiedSegment
        {
            Id = segment.OperatingCarrier.AlternateId,
            Departure = new UnifiedLocation
            {
                AirportCode = segment.Origin.DisplayCode,
                Terminal = null, // SkyScanner doesn't provide terminal information
                CityCode = segment.Origin.Parent?.DisplayCode, // Add city code from parent object
                CountryCode = segment.Origin.Country, // Add country code
                DateTime = segment.Departure,
                FormattedDateTime = FormatDateTime(segment.Departure)
            },
            Arrival = new UnifiedLocation
            {
                AirportCode = segment.Destination.DisplayCode,
                Terminal = null, // SkyScanner doesn't provide terminal information
                CityCode = segment.Destination.Parent?.DisplayCode, // Add city code from parent object
                CountryCode = segment.Destination.Country, // Add country code
                DateTime = segment.Arrival,
                FormattedDateTime = FormatDateTime(segment.Arrival)
            },
            Duration = TimeSpan.FromMinutes(segment.DurationInMinutes),
            FormattedDuration = TimeSpan.FromMinutes(segment.DurationInMinutes).ToString(@"hh\:mm"),
            MarketingAirline = segment.MarketingCarrier?.Name,
            OperatingAirline = segment.OperatingCarrier?.Name,
            FlightNumber = $"{segment.MarketingCarrier?.AlternateId} {segment.FlightNumber}",
            AircraftType = "Unknown Aircraft", // Better default than using airline name
            CabinClass = "Economy", // Default as SkyScanner doesn't always provide this
            AirlineLogo = airlineLogo ?? segment.MarketingCarrier?.LogoUrl, // Use provided logo or fallback to airline's logo
            BaggageAllowance = new BaggageAllowance
            {
                CheckedBags = 0,
                WeightKg = null,
                Description = "No checked baggage information available"
            }
        };
    }

    // New helper methods to extract additional information
    private static Dictionary<string, string> ExtractAirlines(SkyScannerFlightOffer response)
    {
        var airlines = new Dictionary<string, string>();

        foreach (var itinerary in response.Data.Itineraries)
        {
            foreach (var leg in itinerary.Legs)
            {
                foreach (var carrier in leg.Carriers.Marketing)
                {
                    if (!string.IsNullOrEmpty(carrier.AlternateId) && !airlines.ContainsKey(carrier.AlternateId))
                    {
                        airlines[carrier.AlternateId] = carrier.Name;
                    }
                }

                foreach (var segment in leg.Segments)
                {
                    if (segment.MarketingCarrier != null && !string.IsNullOrEmpty(segment.MarketingCarrier.AlternateId)
                        && !airlines.ContainsKey(segment.MarketingCarrier.AlternateId))
                    {
                        airlines[segment.MarketingCarrier.AlternateId] = segment.MarketingCarrier.Name;
                    }

                    if (segment.OperatingCarrier != null && !string.IsNullOrEmpty(segment.OperatingCarrier.AlternateId)
                        && !airlines.ContainsKey(segment.OperatingCarrier.AlternateId))
                    {
                        airlines[segment.OperatingCarrier.AlternateId] = segment.OperatingCarrier.Name;
                    }
                }
            }
        }

        return airlines;
    }

    private static Dictionary<string, string> ExtractAirports(SkyScannerFlightOffer response)
    {
        var airports = new Dictionary<string, string>();

        foreach (var itinerary in response.Data.Itineraries)
        {
            foreach (var leg in itinerary.Legs)
            {
                if (!string.IsNullOrEmpty(leg.Origin.DisplayCode) && !airports.ContainsKey(leg.Origin.DisplayCode))
                {
                    airports[leg.Origin.DisplayCode] = leg.Origin.Name;
                }

                if (!string.IsNullOrEmpty(leg.Destination.DisplayCode) && !airports.ContainsKey(leg.Destination.DisplayCode))
                {
                    airports[leg.Destination.DisplayCode] = leg.Destination.Name;
                }

                foreach (var segment in leg.Segments)
                {
                    if (!string.IsNullOrEmpty(segment.Origin.DisplayCode) && !airports.ContainsKey(segment.Origin.DisplayCode))
                    {
                        airports[segment.Origin.DisplayCode] = segment.Origin.Name;
                    }

                    if (!string.IsNullOrEmpty(segment.Destination.DisplayCode) && !airports.ContainsKey(segment.Destination.DisplayCode))
                    {
                        airports[segment.Destination.DisplayCode] = segment.Destination.Name;
                    }
                }
            }
        }

        return airports;
    }

    private static Dictionary<string, AirportCity> ExtractLocations(SkyScannerFlightOffer response)
    {
        var locations = new Dictionary<string, AirportCity>();

        foreach (var itinerary in response.Data.Itineraries)
        {
            foreach (var leg in itinerary.Legs)
            {
                if (!string.IsNullOrEmpty(leg.Origin.DisplayCode) && !locations.ContainsKey(leg.Origin.DisplayCode))
                {
                    locations[leg.Origin.DisplayCode] = new AirportCity
                    {
                        CityCode = leg.Origin.City,
                        CityName = leg.Origin.City,
                        CountryCode = leg.Origin.Country,
                        CountryName = leg.Origin.Country
                    };
                }

                if (!string.IsNullOrEmpty(leg.Destination.DisplayCode) && !locations.ContainsKey(leg.Destination.DisplayCode))
                {
                    locations[leg.Destination.DisplayCode] = new AirportCity
                    {
                        CityCode = leg.Destination.City,
                        CityName = leg.Destination.City,
                        CountryCode = leg.Destination.Country,
                        CountryName = leg.Destination.Country
                    };
                }

                foreach (var segment in leg.Segments)
                {
                    if (segment.Origin.Parent != null && !string.IsNullOrEmpty(segment.Origin.DisplayCode) &&
                        !locations.ContainsKey(segment.Origin.DisplayCode))
                    {
                        locations[segment.Origin.DisplayCode] = new AirportCity
                        {
                            CityCode = segment.Origin.Parent.DisplayCode,
                            CityName = segment.Origin.Parent.Name,
                            CountryCode = segment.Origin.Country,
                            CountryName = segment.Origin.Country
                        };
                    }

                    if (segment.Destination.Parent != null && !string.IsNullOrEmpty(segment.Destination.DisplayCode) &&
                        !locations.ContainsKey(segment.Destination.DisplayCode))
                    {
                        locations[segment.Destination.DisplayCode] = new AirportCity
                        {
                            CityCode = segment.Destination.Parent.DisplayCode,
                            CityName = segment.Destination.Parent.Name,
                            CountryCode = segment.Destination.Country,
                            CountryName = segment.Destination.Country
                        };
                    }
                }
            }
        }

        return locations;
    }
}