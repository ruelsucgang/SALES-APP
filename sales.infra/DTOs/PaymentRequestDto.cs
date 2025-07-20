namespace sales.infra.DTOs
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PHP";
        public string Description { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "card"; // card, gcash, paymaya, grab_pay
        public CustomerDetailsDto Customer { get; set; } = new CustomerDetailsDto();
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    public class CustomerDetailsDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}

// example JSON Request:
//{
//    "amount": 1500.00,
//  "currency": "PHP",
//  "description": "Order #5 - Sales Application",
//  "paymentMethod": "gcash",
//  "customer": {
//        "name": "Juan Dela Cruz",
//    "email": "juan@example.com",
//    "phone": "+639171234567"
//  },
//  "metadata": {
//        "order_id": "5",
//    "customer_id": "10"
//  }
//}