using System;
using System.Collections.Generic;

namespace sales.domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled

        public string? PaymentIntentId { get; set; } // Stripe Payment Intent ID

        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Failed

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}