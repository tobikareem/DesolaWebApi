using AutoMapper;
using CaptainPayment.Core.Exceptions;
using CaptainPayment.Core.Interfaces;
using CaptainPayment.Core.Models;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Services;

public class CustomerManagementService : ICustomerManagementService
{
    private readonly ICustomerTableService _customerTableService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CustomerManagementService> _logger;
    private readonly ICustomerService _stripeCustomerService;
    private readonly IMapper _mapper;

    public CustomerManagementService(ICustomerTableService table, ICacheService cacheService, ILogger<CustomerManagementService> logger, ICustomerService customerService, IMapper mapper)
    {
        _customerTableService = table;
        _cacheService=cacheService;
        _logger=logger;
        _stripeCustomerService = customerService;
        _mapper = mapper;
    }

    public async Task<Customer> GetByEmailAsync(string email)
        => await _customerTableService.GetByEmailAsync(email);

    public async Task<Customer> GetByStripeCustomerIdAsync(string stripeCustomerId)
        => await _customerTableService.GetByStripeCustomerIdAsync(stripeCustomerId);

    public async Task<IEnumerable<Customer>> GetActiveSubscribersAsync()
        => await _customerTableService.GetActiveSubscribersAsync();

    public async Task<bool> UpdateSubscriptionStatusAsync(string stripeCustomerId, bool hasActiveSubscription, DateTime? subscriptionExpiresAt = null, string subscriptionId = null)
        => await _customerTableService.UpdateSubscriptionStatusAsync(stripeCustomerId, hasActiveSubscription, subscriptionExpiresAt, subscriptionId);

    public async Task<IEnumerable<Customer>> GetCustomersByDomainAsync(string domain, int limit = 100)
        => await _customerTableService.GetCustomersByDomainAsync(domain, limit);

    public async Task<CustomerCreationResult> CreateCustomerAsync(CustomerSignupRequest customer, CancellationToken tokenSource)
    {
        ValidateCreateRequest(customer);

        try
        {
            _logger.LogInformation("Starting customer creation process for email: {Email}", customer.Email);
            
            var existingCustomer = await _customerTableService.GetByEmailAsync(customer.Email);
            if (existingCustomer != null)
            {
                _logger.LogWarning("Customer already exists with email: {Email}", customer.Email);
                return new CustomerCreationResult
                {
                    Success = false,
                    ErrorMessage = "Customer with this email already exists",
                    Customer = existingCustomer
                };
            }

            var stripeCustomerRequest = _mapper.Map<CreateCustomerRequest>(customer);
            var stripeCustomer = await _stripeCustomerService.CreateCustomerAsync(stripeCustomerRequest, tokenSource);
            _logger.LogInformation("Stripe customer created: {StripeCustomerId}", stripeCustomer.Id);

            var localCustomer = _mapper.Map<Customer>(customer);
            localCustomer.StripeCustomerId = stripeCustomer.Id;

            await _customerTableService.InsertTableEntityAsync(localCustomer);
            _logger.LogInformation("Local customer record created for email: {Email}", customer.Email);

            return new CustomerCreationResult
            {
                Success = true,
                Customer = localCustomer,
                StripeCustomerId = stripeCustomer.Id
            };
        }
        catch (PaymentException ex)
        {
            _logger.LogError(ex, "Stripe error during customer creation: {Email}", customer.Email);
            return new CustomerCreationResult
            {
                Success = false,
                ErrorMessage = $"Payment service error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during customer creation: {Email}", customer.Email);
            return new CustomerCreationResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred during customer creation"
            };
        }
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        customer.LastActiveAt = DateTime.UtcNow;

        var existingCustomer = await GetByEmailAsync(customer.Email);

        if (existingCustomer == null)
        {
            throw new KeyNotFoundException($"Customer with email {customer.Email} not found.");
        }


        try
        {
            await _customerTableService.UpdateTableEntityAsync(customer);
            _logger.LogInformation("Customer updated successfully: {Email}", customer.Email);

            if (string.IsNullOrEmpty(customer.StripeCustomerId))
            {
                return customer;
            }
            var cacheKey = $"stripe_customer_{customer.StripeCustomerId}";
            _cacheService.Add(cacheKey, customer, TimeSpan.FromHours(1));

            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update customer: {Email}", customer.Email);
            throw;
        }
    }

    private static void ValidateCreateRequest(CustomerSignupRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Name is required", nameof(request));
    }
}