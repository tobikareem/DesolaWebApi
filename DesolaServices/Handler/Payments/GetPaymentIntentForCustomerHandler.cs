using DesolaDomain.Entities.Payment;
using DesolaServices.Commands.Queries.Payment;
using DesolaServices.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesolaServices.Handler.Payments;

public class GetPaymentIntentForCustomerHandler : IRequestHandler<GetSetupIntentQuery, IEnumerable<PaymentIntentResult>>
{
    private readonly ILogger<GetPaymentIntentForCustomerHandler> _logger;
    private readonly IPaymentIntentResultService _paymentIntentResultService;
    public GetPaymentIntentForCustomerHandler(ILogger<GetPaymentIntentForCustomerHandler> logger, IPaymentIntentResultService paymentIntentResultService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
        _paymentIntentResultService = paymentIntentResultService;
    }

    public async Task<IEnumerable<PaymentIntentResult>> Handle(GetSetupIntentQuery request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        if (string.IsNullOrWhiteSpace(request.CustomerId))
            throw new ArgumentException("CustomerId cannot be null or empty", nameof(request.CustomerId));

        _logger.LogInformation("Handling GetSetupIntentQuery for CustomerId: {CustomerId}", request.CustomerId);
        var paymentIntentResult = await _paymentIntentResultService.GetByCustomerIdAsync(request.CustomerId);
        if (paymentIntentResult == null)
        {
            _logger.LogWarning("No payment intent found for CustomerId: {CustomerId}", request.CustomerId);
            return null; 
        }
        _logger.LogInformation("Found payment intent for CustomerId: {CustomerId}", request.CustomerId);

        return paymentIntentResult;
    }
}