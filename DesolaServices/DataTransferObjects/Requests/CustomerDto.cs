
using DesolaDomain.Entities.User;

namespace DesolaServices.DataTransferObjects.Requests;
public class CustomerDto
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string PreferredCurrency { get; set; }
    public string DefaultOriginAirport { get; set; }
    public bool HasActiveSubscription { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }

    public static CustomerDto FromCustomer(Customer customer)
    {
        return new CustomerDto
        {
            Email = customer.Email,
            FullName = customer.FullName,
            Phone = customer.Phone,
            PreferredCurrency = customer.PreferredCurrency,
            DefaultOriginAirport = customer.DefaultOriginAirport,
            HasActiveSubscription = customer.HasActiveSubscription,
            SubscriptionExpiresAt = customer.SubscriptionExpiresAt,
            CreatedAt = customer.CreatedAt,
            Status = customer.Status.ToString()
        };
    }
}