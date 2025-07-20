using System;

namespace sales.infra.DTOs
{
    public class PaymentResponseDto
    {
        public string Id { get; set; } = string.Empty; // Payment reference ID
        public string Status { get; set; } = string.Empty; // pending, awaiting_payment, paid, failed, expired
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PHP";
        public string PaymentMethod { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? CheckoutUrl { get; set; } // For GCash/PayMaya: redirect URL
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        public PaymentDetailsDto? PaymentDetails { get; set; }
    }

    public class PaymentDetailsDto
    {
        public string? TransactionId { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? Source { get; set; } // gcash, paymaya, card
        public CardDetailsDto? Card { get; set; }
    }

    public class CardDetailsDto
    {
        public string Last4 { get; set; } = string.Empty; // Last 4 digits
        public string Brand { get; set; } = string.Empty; // visa, mastercard
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
    }
}

// example JSON Response (Mock):
//{
//    "id": "pay_mock_abc123def456",
//  "status": "paid",
//  "amount": 1500.00,
//  "currency": "PHP",
//  "paymentMethod": "gcash",
//  "description": "Order #5 - Sales Application",
//  "createdAt": "2025-07-03T10:00:00Z",
//  "paidAt": "2025-07-03T10:02:35Z",
//  "checkoutUrl": null,
//  "metadata": {
//        "order_id": "5",
//    "customer_id": "10"
//  },
//  "paymentDetails": {
//        "transactionId": "GCH-TXN-20250703-123456",
//    "receiptNumber": "RCP-789012",
//    "source": "gcash",
//    "card": null
//  }
//}