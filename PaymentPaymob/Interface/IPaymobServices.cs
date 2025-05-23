using PaymentPaymob.Models;

namespace PaymentPaymob.Interface
{
    public interface IPaymobServices
    {
        Task<PaymentResponseDto> InitiatePaymentAsync(PaymentRequestDto request);
        Task<PaymentCallbackDto?> ProcessPaymentCallbackAsync(string payload, string HmacResevied);
    }
}
