using Microsoft.AspNetCore.Mvc;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.API.Controllers
{
    [Route("api/customer-auth")]
    [ApiController]
    public class CustomerAuthController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IOtpService _otpService;

        public CustomerAuthController(ICustomerService customerService, IOtpService otpService)
        {
            _customerService = customerService;
            _otpService = otpService;
        }

        // POST: api/customer-auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CustomerRegisterRequestDto request)
        {
            try
            {
                var customer = await _customerService.RegisterCustomerAsync(request);
                return CreatedAtAction(nameof(Register), new { id = customer.Id }, customer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/request-otp
        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto request)
        {
            try
            {
                var success = await _otpService.RequestOtpAsync(request.Email);
                if (!success)
                {
                    return NotFound(new { message = "Customer email not found." });
                }

                return Ok(new { message = "OTP has been sent to your email." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/customer-auth/verify-otp
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            var token = await _otpService.VerifyOtpAsync(request.Email, request.Code);

            if (token == null)
            {
                return BadRequest(new { message = "Invalid or expired OTP code." });
            }

            return Ok(new
            {
                token = token,
                expiresIn = 3600 // 60 minutes in seconds
            });
        }

        // POST: api/customer-auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // JWT is stateless - client handles token deletion
            return Ok(new { message = "Logged out successfully. Please delete your token on the client side." });
        }
    }
}