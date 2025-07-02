using AutoMapper;
using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;
using DesolaDomain.Model;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class CustomerStripeProfile : Profile
{
    public CustomerStripeProfile()
    {
        CreateMap<Customer, CreateCustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "Desola Flights Customer"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateCustomerMetadata(src)));


        // Map CustomerSignupRequest to CreateCustomerRequest for Stripe API
        CreateMap<CustomerSignupRequest, CreateCustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => "Desola Flights Customer"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateSignupMetadata(src)));


        // Map CustomerResult (from Stripe) to Customer entity
        CreateMap<CustomerResult, Customer>()
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .AfterMap((src, dest) => SetTableStorageKeys(dest));

        // Map CustomerSignupRequest to Customer entity
        CreateMap<CustomerSignupRequest, Customer>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.PreferredCurrency ?? "USD"))
            .ForMember(dest => dest.DefaultOriginAirport, opt => opt.MapFrom(src => src.DefaultOriginAirport))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
            .AfterMap((src, dest) => SetTableStorageKeys(dest));

        // Map CreateCustomerRequest to Customer entity
        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
            .AfterMap((src, dest) => SetTableStorageKeys(dest));

        // Map UpdateCustomerRequest to Customer entity
        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow)); // Don't overwrite other properties

        CreateMap<Customer, CustomerResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.MapFrom(src => src.HasActiveSubscription))
            .ForMember(dest => dest.SubscriptionExpiresAt, opt => opt.MapFrom(src => src.SubscriptionExpiresAt))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Map PagedResult<CustomerResult> to PagedResult<Customer>
        CreateMap<PagedResult<CustomerResult>, PagedResult<Customer>>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data));

        CreateMap<NewUserSignUpCommand, CustomerSignupRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.PreferredCurrency))
            .ForMember(dest => dest.DefaultOriginAirport, opt => opt.MapFrom(src => src.DefaultOriginAirport))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId));

    }

    private static Dictionary<string, string> CreateCustomerMetadata(Customer customer)
    {
        return new Dictionary<string, string>
        {
            { "source", "desola_flights" },
            { "signup_date", DateTime.UtcNow.ToString("yyyy-MM-dd") },
            { "preferred_currency", customer.PreferredCurrency ?? "USD" },
            { "default_origin", customer.DefaultOriginAirport ?? "" }
        };
    }

    private static Dictionary<string, string> CreateSignupMetadata(CustomerSignupRequest request)
    {
        var metadata = new Dictionary<string, string>
        {
            { "source", "desola_flights" },
            { "signup_date", DateTime.UtcNow.ToString("yyyy-MM-dd") },
            { "preferred_currency", request.PreferredCurrency ?? "USD" }
        };

        if (!string.IsNullOrEmpty(request.DefaultOriginAirport))
            metadata.Add("default_origin", request.DefaultOriginAirport);

        // Add any custom metadata from the request
        if (request.Metadata?.Any() == true)
        {
            foreach (var kvp in request.Metadata)
            {
                if (!metadata.ContainsKey(kvp.Key))
                    metadata.Add(kvp.Key, kvp.Value);
            }
        }

        return metadata;
    }

    private static void SetTableStorageKeys(Customer customer)
    {
        if (!string.IsNullOrEmpty(customer.Email))
        {
            var emailDomain = customer.Email.Split('@').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
            customer.PartitionKey = $"domain_{emailDomain}";
            customer.RowKey = customer.Email.ToLowerInvariant();
        }
    }


}