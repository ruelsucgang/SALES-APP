using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sales.infra.DTOs;
using sales.infra.Interfaces;
using System.Security.Claims;

namespace sales.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentGateway _paymentGateway;

        public OrderController(
            IOrderService orderService,
            ICustomerService customerService,
            IPaymentGateway paymentGateway)
        {
            _orderService = orderService;
            _customerService = customerService;
            _paymentGateway = paymentGateway;
        }

        // post: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // get: api/orders/my-orders
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserIdFromToken();
            var orders = await _orderService.GetCustomerOrdersAsync(userId);
            return Ok(orders);
        }

        // get: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetUserIdFromToken();
            var order = await _orderService.GetOrderByIdAsync(id, userId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return Ok(order);
        }

        // get: api/orders (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // post: api/orders/{id}/payment
        [HttpPost("{id}/payment")]
        public async Task<IActionResult> CreatePayment(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id, GetUserIdFromToken());
            var customer = await _customerService.GetCustomerByUserIdAsync(GetUserIdFromToken());

            var paymentRequest = new PaymentRequestDto
            {
                Amount = order.TotalAmount,
                Currency = "PHP",
                Description = $"Order #{order.Id}",
                PaymentMethod = "gcash", // or from request
                Customer = new CustomerDetailsDto
                {
                    Name = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.ContactNumber
                },
                Metadata = new Dictionary<string, string>
        {
            { "order_id", order.Id.ToString() },
            { "customer_id", customer.Id.ToString() }
        }
            };

            var response = await _paymentGateway.CreatePaymentAsync(paymentRequest);

            return Ok(new
            {
                paymentId = response.Id,
                status = response.Status,
                checkoutUrl = response.CheckoutUrl,
                amount = response.Amount
            });
        }

        // put: api/orders/{id}/payment-status
        [HttpPut("{id}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto updateDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var order = await _orderService.GetOrderByIdAsync(id, userId);

                if (order == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                // Verify payment with Stripe
                var isPaymentSuccessful = await _paymentGateway.VerifyPaymentAsync(updateDto.PaymentIntentId);

                var paymentStatus = isPaymentSuccessful ? "Paid" : "Failed";

                var updatedOrder = await _orderService.UpdateOrderPaymentStatusAsync(
                    id,
                    updateDto.PaymentIntentId,
                    paymentStatus);

                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // delete: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _orderService.CancelOrderAsync(id, userId);
                return Ok(new { message = "Order cancelled successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Helper method to get UserId from JWT token
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}