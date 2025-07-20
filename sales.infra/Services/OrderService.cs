using sales.domain.Entities;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.infra.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
        }

        public async Task<OrderDto> CreateOrderAsync(int customerId, CreateOrderDto createOrderDto)
        {
            // Validate customer exists
            var customer = await _customerRepository.GetByUserIdAsync(customerId);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            // Validate order has items
            if (createOrderDto.OrderItems == null || !createOrderDto.OrderItems.Any())
            {
                throw new InvalidOperationException("Order must have at least one item.");
            }

            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            decimal totalAmount = 0;

            // Process each order item
            foreach (var itemDto in createOrderDto.OrderItems)
            {
                // Get product details
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {itemDto.ProductId} not found.");
                }

                if (itemDto.Quantity <= 0)
                {
                    throw new InvalidOperationException("Quantity must be greater than zero.");
                }

                var totalPrice = product.Price * itemDto.Quantity;
                totalAmount += totalPrice;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = itemDto.Quantity,
                    TotalPrice = totalPrice
                };

                order.OrderItems.Add(orderItem);
            }

            order.TotalAmount = totalAmount;

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Load order with items and customer for response
            var orderWithDetails = await _orderRepository.GetByIdWithItemsAsync(createdOrder.Id);

            return MapToDto(orderWithDetails!);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id, int customerId)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(id);
            if (order == null)
            {
                return null;
            }

            // Customer can only see their own orders
            var customer = await _customerRepository.GetByUserIdAsync(customerId);
            if (customer != null && order.CustomerId != customer.Id)
            {
                return null; // Not authorized
            }

            return MapToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetCustomerOrdersAsync(int customerId)
        {
            var customer = await _customerRepository.GetByUserIdAsync(customerId);
            if (customer == null)
            {
                return new List<OrderDto>();
            }

            var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id);
            return orders.Select(MapToDto);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto);
        }

        public async Task<OrderDto> UpdateOrderPaymentStatusAsync(int orderId, string paymentIntentId, string paymentStatus)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            order.PaymentIntentId = paymentIntentId;
            order.PaymentStatus = paymentStatus;
            order.Status = paymentStatus == "Paid" ? "Paid" : "Pending";
            order.UpdatedAt = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateAsync(order);
            var orderWithDetails = await _orderRepository.GetByIdWithItemsAsync(updatedOrder.Id);

            return MapToDto(orderWithDetails!);
        }

        public async Task CancelOrderAsync(int orderId, int customerId)
        {
            var order = await _orderRepository.GetByIdWithItemsAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            // Customer can only cancel their own orders
            var customer = await _customerRepository.GetByUserIdAsync(customerId);
            if (customer != null && order.CustomerId != customer.Id)
            {
                throw new InvalidOperationException("Not authorized to cancel this order.");
            }

            if (order.Status == "Paid")
            {
                throw new InvalidOperationException("Cannot cancel a paid order.");
            }

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentIntentId = order.PaymentIntentId,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }
    }
}