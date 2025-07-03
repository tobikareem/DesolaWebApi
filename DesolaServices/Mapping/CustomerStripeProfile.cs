using AutoMapper;
using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Mapping;

public class CustomerStripeProfile : Profile
{
    public CustomerStripeProfile()
    {
        // Map Customer entity to CreateCustomerRequest for Stripe API
        CreateMap<Customer, CreateCustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => $"Desola Flights Customer - {src.FullName}"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateCustomerMetadata(src)))
            .ForMember(dest => dest.DefaultPaymentMethodId, opt => opt.Ignore()) // Not set during initial creation
            .ForMember(dest => dest.Address, opt => opt.Ignore()) // Not implemented yet
            .ForMember(dest => dest.ShippingAddress, opt => opt.Ignore()) // Not implemented yet
            .ForMember(dest => dest.TaxExempt, opt => opt.Ignore()); // Default handling

        // Map CustomerSignupRequest to CreateCustomerRequest for Stripe API
        CreateMap<CustomerSignupRequest, CreateCustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => $"Desola Flights Customer - {src.FullName}"))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateSignupMetadata(src)))
            .ForMember(dest => dest.DefaultPaymentMethodId, opt => opt.Ignore())
            .ForMember(dest => dest.Address, opt => opt.Ignore())
            .ForMember(dest => dest.ShippingAddress, opt => opt.Ignore())
            .ForMember(dest => dest.TaxExempt, opt => opt.Ignore());

        // Map CustomerResult (from Stripe) to Customer entity - this is for updates/syncing
        CreateMap<CustomerResult, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Keep existing ID
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Keep existing ID
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name ?? string.Empty))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Created))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.Currency ?? "USD"))

            // Preserve local-only fields that Stripe doesn't manage
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.Ignore())
            .ForMember(dest => dest.SubscriptionExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentSubscriptionId, opt => opt.Ignore())
            .ForMember(dest => dest.DefaultOriginAirport, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())

            // Handle metadata carefully
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => MergeMetadata(src.Metadata)))
            .ForMember(dest => dest.MetadataJson, opt => opt.Ignore()) // Will be set by Metadata property

            // Azure Table Storage properties - will be set in AfterMap
            .ForMember(dest => dest.PartitionKey, opt => opt.Ignore())
            .ForMember(dest => dest.RowKey, opt => opt.Ignore())
            .ForMember(dest => dest.ETag, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .AfterMap((src, dest) => dest.SetTableStorageKeys());

        // Map CustomerSignupRequest to Customer entity - for new customer creation
        CreateMap<CustomerSignupRequest, Customer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
            .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.PreferredCurrency ?? "USD"))
            .ForMember(dest => dest.DefaultOriginAirport, opt => opt.MapFrom(src => src.DefaultOriginAirport ?? string.Empty))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.SubscriptionExpiresAt, opt => opt.MapFrom(src => (DateTime?)null))
            .ForMember(dest => dest.CurrentSubscriptionId, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.Ignore()) // Will be set after Stripe creation
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateCustomerMetadataFromSignup(src)))
            .ForMember(dest => dest.MetadataJson, opt => opt.Ignore()) // Will be set by Metadata property
            .ForMember(dest => dest.ETag, opt => opt.Ignore())
            .ForMember(dest => dest.Timestamp, opt => opt.Ignore())
            .AfterMap((src, dest) => dest.SetTableStorageKeys());

        // Map NewUserSignUpCommand to CustomerSignupRequest
        CreateMap<NewUserSignUpCommand, CustomerSignupRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
            .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.PreferredCurrency ?? "USD"))
            .ForMember(dest => dest.DefaultOriginAirport, opt => opt.MapFrom(src => src.DefaultOriginAirport ?? string.Empty))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata ?? new Dictionary<string, string>()));

        // Map Customer entity to CustomerResponse for API responses
        CreateMap<Customer, CustomerResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.StripeCustomerId, opt => opt.MapFrom(src => src.StripeCustomerId))
            .ForMember(dest => dest.HasActiveSubscription, opt => opt.MapFrom(src => src.HasActiveSubscription))
            .ForMember(dest => dest.SubscriptionExpiresAt, opt => opt.MapFrom(src => src.SubscriptionExpiresAt))
            .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.PreferredCurrency))
            .ForMember(dest => dest.DefaultOriginAirport, opt => opt.MapFrom(src => src.DefaultOriginAirport))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => src.LastActiveAt))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Map UpdateCustomerRequest to Customer - for partial updates
        CreateMap<UpdateCustomerRequest, Customer>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
            .ForMember(dest => dest.LastActiveAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Metadata,
                opt => opt.MapFrom(src => src.Metadata ?? new Dictionary<string, string>()));

        // Map Customer entity to UpdateCustomerRequest for Stripe updates
        CreateMap<Customer, UpdateCustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => CreateCustomerDescription(src)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => CreateDefaultAddress()))
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => CreateDefaultAddress()))
            .ForMember(dest => dest.DefaultPaymentMethodId, opt => opt.MapFrom(src => string.Empty)) // Not managed locally
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateCustomerMetadata(src)));

        CreateMap<Customer, UpdateCustomerRequest>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => CreateCustomerDescription(src)))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new CustomerAddress())) // Empty address for now
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => new CustomerAddress())) // Empty address for now
            .ForMember(dest => dest.DefaultPaymentMethodId, opt => opt.MapFrom(src => string.Empty)) // Not managed locally
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => CreateCustomerMetadata(src)));

        // Map PagedResult<CustomerResult> to PagedResult<Customer>
        CreateMap<PagedResult<CustomerResult>, PagedResult<Customer>>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.Data))
            .ForMember(dest => dest.HasMore, opt => opt.MapFrom(src => src.HasMore))
            .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.TotalCount));
    }

    /// <summary>
    /// Creates metadata for existing customer when creating Stripe customer
    /// </summary>
    private static Dictionary<string, string> CreateCustomerMetadata(Customer customer)
    {
        var metadata = new Dictionary<string, string>
        {
            ["source"] = "desola_flights",
            ["customer_id"] = customer.Id.ToString(),
            ["signup_date"] = customer.CreatedAt.ToString("yyyy-MM-dd"),
            ["preferred_currency"] = customer.PreferredCurrency,
            ["status"] = customer.Status.ToString(),
            ["member_since"] = customer.CreatedAt.ToString("yyyy-MM-dd")
        };

        if (!string.IsNullOrWhiteSpace(customer.DefaultOriginAirport))
            metadata["default_origin"] = customer.DefaultOriginAirport;

        // Merge with existing metadata
        var existingMetadata = customer.Metadata ?? new Dictionary<string, string>();
        foreach (var kvp in existingMetadata)
        {
            if (!metadata.ContainsKey(kvp.Key))
                metadata[kvp.Key] = kvp.Value;
        }

        return metadata;
    }

    /// <summary>
    /// Creates metadata for new customer signup
    /// </summary>
    private static Dictionary<string, string> CreateSignupMetadata(CustomerSignupRequest request)
    {
        var metadata = new Dictionary<string, string>
        {
            ["source"] = "desola_flights",
            ["signup_date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["preferred_currency"] = request.PreferredCurrency ?? "USD",
            ["signup_method"] = "web_form"
        };

        if (!string.IsNullOrWhiteSpace(request.DefaultOriginAirport))
            metadata["default_origin"] = request.DefaultOriginAirport;

        // Add any custom metadata from the request
        if (request.Metadata?.Any() == true)
        {
            foreach (var kvp in request.Metadata)
            {
                // Avoid overwriting system metadata
                if (!metadata.ContainsKey(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Key))
                    metadata[kvp.Key] = kvp.Value ?? string.Empty;
            }
        }

        return metadata;
    }

    /// <summary>
    /// Creates customer metadata during customer entity creation from signup
    /// </summary>
    private static Dictionary<string, string> CreateCustomerMetadataFromSignup(CustomerSignupRequest request)
    {
        var metadata = new Dictionary<string, string>
        {
            ["source"] = "desola_flights",
            ["created_date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            ["preferred_currency"] = request.PreferredCurrency ?? "USD",
            ["signup_method"] = "web_form"
        };

        if (!string.IsNullOrWhiteSpace(request.DefaultOriginAirport))
            metadata["default_origin"] = request.DefaultOriginAirport;

        // Add custom metadata from request
        if (request.Metadata?.Any() == true)
        {
            foreach (var kvp in request.Metadata)
            {
                if (!metadata.ContainsKey(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Key))
                    metadata[kvp.Key] = kvp.Value ?? string.Empty;
            }
        }

        return metadata;
    }

    /// <summary>
    /// Merges Stripe metadata with system metadata, preserving system values
    /// </summary>
    private static Dictionary<string, string> MergeMetadata(Dictionary<string, string> stripeMetadata)
    {
        var metadata = stripeMetadata ?? new Dictionary<string, string>();

        // Ensure system metadata exists
        if (!metadata.ContainsKey("source"))
            metadata["source"] = "desola_flights";

        if (!metadata.ContainsKey("last_sync"))
            metadata["last_sync"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        else
            metadata["last_sync"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        return metadata;
    }

    /// <summary>
    /// Creates a descriptive text for the customer in Stripe
    /// </summary>
    private static string CreateCustomerDescription(Customer customer)
    {
        var parts = new List<string> { "Desola Flights Customer" };

        if (customer.HasActiveSubscription)
        {
            parts.Add("Active Subscriber");
        }

        if (!string.IsNullOrWhiteSpace(customer.DefaultOriginAirport))
        {
            parts.Add($"Home Airport: {customer.DefaultOriginAirport}");
        }

        parts.Add($"Member since {customer.CreatedAt:yyyy-MM-dd}");

        return string.Join(" | ", parts);
    }

    /// <summary>
    /// Creates default empty address (since Customer entity doesn't have address fields yet)
    /// </summary>
    private static CustomerAddress CreateDefaultAddress()
    {
        return new CustomerAddress
        {
            Line1 = string.Empty,
            Line2 = string.Empty,
            City = string.Empty,
            State = string.Empty,
            PostalCode = string.Empty,
            Country = string.Empty
        };
    }
}