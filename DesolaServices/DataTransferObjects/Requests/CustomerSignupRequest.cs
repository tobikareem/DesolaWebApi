using System.ComponentModel.DataAnnotations;

namespace DesolaServices.DataTransferObjects.Requests;

public class CustomerSignupRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone format")]
    public string Phone { get; set; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters")]
    public string PreferredCurrency { get; set; } = "USD";

    [StringLength(5, MinimumLength = 3, ErrorMessage = "Airport code must be 3-5 characters")]
    public string DefaultOriginAirport { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    public static bool IsValidEmail(string email)
    {
        try
        {
            var emailAddress = new System.Net.Mail.MailAddress(email);
            return emailAddress.Address == email;
        }
        catch
        {
            return false;
        }
    }
}