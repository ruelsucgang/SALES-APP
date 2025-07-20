using sales.infra.DTOs;

namespace sales.infra.Interfaces
{
    public interface IPaymentGateway
    {
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto request);
        Task<PaymentResponseDto> GetPaymentStatusAsync(string paymentId);
        Task<bool> VerifyPaymentAsync(string paymentId);
        Task<PaymentResponseDto> CancelPaymentAsync(string paymentId);
    }
}