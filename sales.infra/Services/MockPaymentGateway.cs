using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.infra.Services
{
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly Dictionary<string, PaymentResponseDto> _mockPayments = new();

        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto request)
        {
            // simulate API delay
            await Task.Delay(500);

            // generate mock payment ID
            var paymentId = $"pay_mock_{Guid.NewGuid().ToString("N").Substring(0, 16)}";

            // create mock response
            var response = new PaymentResponseDto
            {
                Id = paymentId,
                Status = DeterminePaymentStatus(request.PaymentMethod),
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                PaidAt = null,
                CheckoutUrl = GenerateCheckoutUrl(paymentId, request.PaymentMethod),
                Metadata = request.Metadata,
                PaymentDetails = GenerateMockPaymentDetails(request.PaymentMethod)
            };

            // if auto-success, mark as paid
            if (response.Status == "paid")
            {
                response.PaidAt = DateTime.UtcNow.AddSeconds(5);
            }

            // Store in mock database
            _mockPayments[paymentId] = response;

            return response;
        }

        public async Task<PaymentResponseDto> GetPaymentStatusAsync(string paymentId)
        {
            await Task.Delay(200);

            if (_mockPayments.TryGetValue(paymentId, out var payment))
            {
                // Simulate payment completion for pending payments
                if (payment.Status == "awaiting_payment" && ShouldAutoComplete())
                {
                    payment.Status = "paid";
                    payment.PaidAt = DateTime.UtcNow;
                }

                return payment;
            }

            throw new InvalidOperationException($"Payment {paymentId} not found.");
        }

        public async Task<bool> VerifyPaymentAsync(string paymentId)
        {
            var payment = await GetPaymentStatusAsync(paymentId);
            return payment.Status == "paid";
        }

        public async Task<PaymentResponseDto> CancelPaymentAsync(string paymentId)
        {
            await Task.Delay(200);

            if (_mockPayments.TryGetValue(paymentId, out var payment))
            {
                if (payment.Status == "paid")
                {
                    throw new InvalidOperationException("Cannot cancel a paid payment.");
                }

                payment.Status = "cancelled";
                return payment;
            }

            throw new InvalidOperationException($"Payment {paymentId} not found.");
        }

        // helper methods for mock behavior
        private string DeterminePaymentStatus(string paymentMethod)
        {
            // cards auto-succeed (for testing)
            if (paymentMethod == "card")
                return "paid";

            // e-wallets require checkout (simulated)
            if (paymentMethod == "gcash" || paymentMethod == "paymaya" || paymentMethod == "grab_pay")
                return "awaiting_payment";

            return "pending";
        }

        private string? GenerateCheckoutUrl(string paymentId, string paymentMethod)
        {
            // e-wallets need checkout URLs
            if (paymentMethod == "gcash" || paymentMethod == "paymaya" || paymentMethod == "grab_pay")
            {
                return $"https://mock-payment-gateway.local/checkout/{paymentId}";
            }

            return null;
        }

        private PaymentDetailsDto GenerateMockPaymentDetails(string paymentMethod)
        {
            return new PaymentDetailsDto
            {
                TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}",
                ReceiptNumber = $"RCP-{new Random().Next(100000, 999999)}",
                Source = paymentMethod,
                Card = paymentMethod == "card" ? new CardDetailsDto
                {
                    Last4 = "4242",
                    Brand = "visa",
                    ExpiryMonth = 12,
                    ExpiryYear = 2026
                } : null
            };
        }

        private bool ShouldAutoComplete()
        {
            // 80% chance to auto-complete for testing
            return new Random().Next(100) < 80;
        }
    }
}