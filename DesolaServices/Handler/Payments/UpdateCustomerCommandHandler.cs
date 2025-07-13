using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerUpdateResponse>
{
    private readonly ICustomerManagementService _customerManagementService;
    private readonly ICustomerTableService _customerTableService;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;
    public UpdateCustomerCommandHandler(
        ICustomerManagementService customerManagementService,
        ILogger<UpdateCustomerCommandHandler> logger, ICustomerTableService customerTableService)
    {
        _customerManagementService = customerManagementService ?? throw new ArgumentNullException(nameof(customerManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _customerTableService = customerTableService ?? throw new ArgumentNullException(nameof(customerTableService));
    }

    public async Task<CustomerUpdateResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            _logger.LogError("UpdateCustomerCommand request is null");
            return CustomerUpdateResponse.FailureResult("Request cannot be null");
        }

        try
        {
            _logger.LogInformation("Processing customer update for email: {Email}", request.Email);

            // 1. Validate the command
            if (!request.IsValid(out var validationErrors))
            {
                _logger.LogWarning("Validation failed for update request: {Email}. Errors: {Errors}",
                    request.Email, string.Join(", ", validationErrors));
                return CustomerUpdateResponse.ValidationFailureResult(validationErrors);
            }

            // 2. Get existing customer
            var existingCustomer = await GetCustomerForUpdate(request.Email, cancellationToken);
            if (existingCustomer == null)
            {
                _logger.LogWarning("Customer not found for email: {Email}", request.Email);
                return CustomerUpdateResponse.NotFoundResult(request.Email);
            }

            // 3. Track what fields are being updated
            var updatedFields = request.GetUpdatedFields();
            _logger.LogInformation("Updating fields for customer {Email}: {Fields}",
                request.Email, string.Join(", ", updatedFields));

            // 4. Apply updates to the customer object
            var updatedCustomer = ApplyUpdates(existingCustomer, request, out var hasExistingChange);

            // 5. Validate the updated customer
            if (!updatedCustomer.IsValid(out var customerValidationErrors))
            {
                _logger.LogError("Customer validation failed after applying updates: {Email}. Errors: {Errors}",
                    request.Email, string.Join(", ", customerValidationErrors));
                return CustomerUpdateResponse.ValidationFailureResult(customerValidationErrors);
            }

            // If no changes were made, return success without further processing
            if (!hasExistingChange)
            {
                _logger.LogInformation("No changes detected for customer {Email}. No update needed.", request.Email);
                return CustomerUpdateResponse.SuccessResult(updatedCustomer, updatedFields);
            }

            // 6. Update in local storage and sync with Stripe if needed
            var success = await _customerManagementService.UpdateCustomerProfileAsync(updatedCustomer, request.GetUpdatedFields(), cancellationToken);

            if (!success) return CustomerUpdateResponse.FailureResult("Failed to update customer");
            updatedCustomer = await _customerManagementService.GetCustomerAsync(request.Email, cancellationToken);
            return CustomerUpdateResponse.SuccessResult(updatedCustomer, request.GetUpdatedFields(), true);

        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Customer not found during update: {Email}", request.Email);
            return CustomerUpdateResponse.NotFoundResult(request.Email);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument during customer update: {Email}", request.Email);
            return CustomerUpdateResponse.FailureResult("Invalid input provided");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation during customer update: {Email}", request.Email);
            return CustomerUpdateResponse.FailureResult("Unable to process update at this time");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during customer update: {Email}", request.Email);
            return CustomerUpdateResponse.FailureResult("An unexpected error occurred during update");
        }
    }

    private static Customer ApplyUpdates(Customer existingCustomer, UpdateCustomerCommand request, out bool hasUpdate)
    {
        if (existingCustomer == null)
            throw new ArgumentNullException(nameof(existingCustomer));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        hasUpdate = false;
        // Only update fields that are provided (not null/empty)
        if (!string.IsNullOrWhiteSpace(request.FullName) && !string.Equals(existingCustomer.FullName.Trim(), request.FullName.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            existingCustomer.FullName = request.FullName.Trim();
            hasUpdate = true;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone) && !string.Equals(existingCustomer.Phone.Trim(), request.Phone.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            existingCustomer.Phone = request.Phone.Trim();
            hasUpdate = true;
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredCurrency) && !string.Equals(existingCustomer.PreferredCurrency.Trim(), request.PreferredCurrency.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            existingCustomer.PreferredCurrency = request.PreferredCurrency.Trim().ToUpperInvariant();
            hasUpdate = true;
        }

        if (!string.IsNullOrWhiteSpace(request.DefaultOriginAirport) && !string.Equals(existingCustomer.DefaultOriginAirport.Trim(), request.DefaultOriginAirport.Trim(), StringComparison.CurrentCultureIgnoreCase))
        {
            existingCustomer.DefaultOriginAirport = request.DefaultOriginAirport.Trim().ToUpperInvariant();
            hasUpdate = true;
        }

        // Handle metadata updates
        if (request.Metadata?.Any() == true)
        {
            var currentMetadata = existingCustomer.Metadata ?? new Dictionary<string, string>();

            foreach (var (key, value) in request.Metadata)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;
                if (string.IsNullOrEmpty(value))
                {
                    currentMetadata.Remove(key);
                }
                else
                {
                    currentMetadata[key] = value;
                }
            }

            existingCustomer.Metadata = currentMetadata;
            hasUpdate = true;
        }

        // Update timestamp and metadata
        existingCustomer.LastActiveAt = DateTime.UtcNow;
        existingCustomer.SetMetadata("last_updated", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        existingCustomer.SetMetadata("update_source", "admin_portal");

        return existingCustomer;
    }

    private async Task<Customer> GetCustomerForUpdate(string email, CancellationToken cancellationToken)
    {

        var emailDomain = email.Split('@').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
        var partitionKey = $"domain_{emailDomain}";
        var rowKey = email.ToLowerInvariant();

        var localCustomer = await _customerTableService.GetTableEntityAsync(partitionKey, rowKey);

        if (localCustomer?.StripeCustomerId != null)
        {
            return await _customerManagementService.GetCustomerByStripeIdAsync(localCustomer.StripeCustomerId, cancellationToken);
        }

        return localCustomer;
    }
}