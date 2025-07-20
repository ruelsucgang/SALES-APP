using sales.infra.DTOs;

namespace sales.infra.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(int customerId, CreateOrderDto createOrderDto);
        Task<OrderDto?> GetOrderByIdAsync(int id, int customerId);
        Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(int customerId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> UpdateOrderPaymentStatusAsync(int orderId, string paymentIntentId, string paymentStatus);
        Task CancelOrderAsync(int orderId, int customerId);
    }
}