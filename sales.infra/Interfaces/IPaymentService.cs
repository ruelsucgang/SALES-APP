namespace sales.infra.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntentAsync(decimal amount, int orderId, string customerEmail);
        Task<bool> ConfirmPaymentAsync(string paymentIntentId);
        Task<string> GetPaymentStatusAsync(string paymentIntentId);
    }
}