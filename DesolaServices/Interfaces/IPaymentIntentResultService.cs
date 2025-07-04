using DesolaDomain.Entities.Payment;
using System.Threading.Tasks;
using CaptainPayment.Core.Models;

namespace DesolaServices.Interfaces;

public interface IPaymentIntentResultService
{
    /// <summary>
    /// Get payment intent by Setup Customer ID
    /// </summary>
    Task<IEnumerable<PaymentIntentResult>> GetByCustomerIdAsync(string customerId, int monthsBack = 12);

    /// <summary>
    /// save a  payment intent status
    /// </summary>
    Task SavePaymentIntentAsync(SetupIntentResult paymentIntent , string userId);
}