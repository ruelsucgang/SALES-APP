namespace sales.infra.DTOs
{
    public class CustomerUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }
}