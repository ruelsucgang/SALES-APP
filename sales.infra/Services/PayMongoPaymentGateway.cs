using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.infra.Services
{
    public class PayMongoPaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.paymongo.com/v1";

        public PayMongoPaymentGateway(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["PayMongoSettings:SecretKey"] ?? throw new ArgumentNullException("PayMongo SecretKey not configured");

            var authBytes = Encoding.UTF8.GetBytes($"{_apiKey}:");
            var base64Auth = Convert.ToBase64String(authBytes);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto request)
        {
            // Convert to PayMongo format
            var paymongoRequest = new
            {
                data = new
                {
                    attributes = new
                    {
                        amount = (int)(request.Amount * 100), // Convert to centavos
                        currency = request.Currency.ToLower(),
                        description = request.Description,
                        statement_descriptor = "Sales App",
                        metadata = request.Metadata
                    }
                }
            };

            var json = JsonSerializer.Serialize(paymongoRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/payment_intents", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var paymongoResponse = JsonSerializer.Deserialize<PayMongoPaymentIntentResponse>(responseJson);

            // Map to standard response
            return MapToStandardResponse(paymongoResponse);
        }

        public async Task<PaymentResponseDto> GetPaymentStatusAsync(string paymentId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/payment_intents/{paymentId}");
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var paymongoResponse = JsonSerializer.Deserialize<PayMongoPaymentIntentResponse>(responseJson);

            return MapToStandardResponse(paymongoResponse);
        }

        public async Task<bool> VerifyPaymentAsync(string paymentId)
        {
            var payment = await GetPaymentStatusAsync(paymentId);
            return payment.Status == "paid";
        }

        public async Task<PaymentResponseDto> CancelPaymentAsync(string paymentId)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/payment_intents/{paymentId}/cancel", null);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var paymongoResponse = JsonSerializer.Deserialize<PayMongoPaymentIntentResponse>(responseJson);

            return MapToStandardResponse(paymongoResponse);
        }

        private PaymentResponseDto MapToStandardResponse(PayMongoPaymentIntentResponse? paymongoResponse)
        {
            // Map PayMongo response to your standard format
            return new PaymentResponseDto
            {
                Id = paymongoResponse?.Data?.Id ?? string.Empty,
                Status = MapPayMongoStatus(paymongoResponse?.Data?.Attributes?.Status ?? ""),
                Amount = (paymongoResponse?.Data?.Attributes?.Amount ?? 0) / 100m,
                Currency = paymongoResponse?.Data?.Attributes?.Currency?.ToUpper() ?? "PHP",
                // ... map other fields
            };
        }

        private string MapPayMongoStatus(string paymongoStatus)
        {
            return paymongoStatus switch
            {
                "awaiting_payment_method" => "pending",
                "awaiting_next_action" => "awaiting_payment",
                "processing" => "processing",
                "succeeded" => "paid",
                "failed" => "failed",
                "cancelled" => "cancelled",
                _ => "unknown"
            };
        }
    }

    // payMongo response models
    public class PayMongoPaymentIntentResponse
    {
        public PayMongoData? Data { get; set; }
    }

    public class PayMongoData
    {
        public string Id { get; set; } = string.Empty;
        public PayMongoAttributes? Attributes { get; set; }
    }

    public class PayMongoAttributes
    {
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}