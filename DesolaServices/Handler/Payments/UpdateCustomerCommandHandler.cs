using AutoMapper;
using DesolaDomain.Entities.User;
using DesolaServices.Commands.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class UpdateCustomerCommandHandler : IRequest<UpdateCustomerCommand>
{
    private readonly ICustomerManagementService _customerManagementService;
    private readonly ILogger<UpdateCustomerCommandHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateCustomerCommandHandler(
        ICustomerManagementService customerManagementService,
        ILogger<UpdateCustomerCommandHandler> logger,
        IMapper mapper)
    {
        _customerManagementService = customerManagementService ??
                                     throw new ArgumentNullException(nameof(customerManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CustomerUpdateResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
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
            var existingCustomer = await _customerManagementService.GetByEmailAsync(request.Email);
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
            var updatedCustomer = ApplyUpdates(existingCustomer, request);

            // 5. Update in local storage
            var stripeUpdated = await _customerManagementService.UpdateCustomerAsync(updatedCustomer);
            
            return CustomerUpdateResponse.SuccessResult(stripeUpdated, updatedFields, true);
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

    private static Customer ApplyUpdates(Customer existingCustomer, UpdateCustomerCommand request)
    {
        // Only update fields that are provided (not null/empty)
        if (!string.IsNullOrEmpty(request.FullName))
            existingCustomer.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Phone))
            existingCustomer.Phone = request.Phone;

        if (!string.IsNullOrEmpty(request.PreferredCurrency))
            existingCustomer.PreferredCurrency = request.PreferredCurrency;

        if (!string.IsNullOrEmpty(request.DefaultOriginAirport))
            existingCustomer.DefaultOriginAirport = request.DefaultOriginAirport;
        
        if (request.Metadata?.Any() == true)
        {
            existingCustomer.Metadata ??= new Dictionary<string, string>();

            foreach (var kvp in request.Metadata)
            {
                existingCustomer.Metadata[kvp.Key] = kvp.Value;
            }
        }
        
        existingCustomer.LastActiveAt = DateTime.UtcNow;

        return existingCustomer;
    }

}