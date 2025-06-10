using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sales.infra.DTOs;
using sales.infra.Interfaces;
using System.Security.Claims;

namespace sales.API.Controllers
{
    [Route("api/customers")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/customers/me
        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserIdFromToken();
            var customer = await _customerService.GetCustomerByUserIdAsync(userId);

            if (customer == null)
            {
                return NotFound(new { message = "Customer profile not found." });
            }

            return Ok(customer);
        }

        // PUT: api/customers/me
        [HttpPut("me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] CustomerUpdateDto updateDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var updatedCustomer = await _customerService.UpdateCustomerAsync(userId, updateDto);
                return Ok(updatedCustomer);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/customers
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/customers/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = "Customer not found." });
            }

            return Ok(customer);
        }

        // DELETE: api/customers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return Ok(new { message = "Customer deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
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