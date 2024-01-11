using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can access these endpoints
    public class AdminManagementController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AdminManagementController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/adminmanagement/admins
        [HttpGet("admins")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllAdmins()
        {
            var admins = await _userRepository.GetAllAdminsAsync();

            var result = admins.Select(a => new UserDto
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                Role = a.Role,
                IsApproved = a.IsApproved,
                IsBlocked = a.IsBlocked,
                CreatedAt = a.CreatedAt
            });

            return Ok(result);
        }

        // PUT: api/adminmanagement/5/approve
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveAdmin(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Role != "Admin")
            {
                return BadRequest("Only Admin users can be approved.");
            }

            if (user.IsApproved)
            {
                return BadRequest("Admin is already approved.");
            }

            user.IsApproved = true;
            var success = await _userRepository.UpdateAsync(user);

            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to approve admin.");
            }

            return Ok(new { message = "Admin approved successfully." });
        }

        // PUT: api/adminmanagement/5/block
        [HttpPut("{id}/block")]
        public async Task<IActionResult> BlockUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user.Role == "SuperAdmin")
            {
                return BadRequest("Cannot block SuperAdmin.");
            }

            if (user.IsBlocked)
            {
                return BadRequest("User is already blocked.");
            }

            user.IsBlocked = true;
            var success = await _userRepository.UpdateAsync(user);

            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to block user.");
            }

            return Ok(new { message = "User blocked successfully." });
        }

        // PUT: api/adminmanagement/5/unblock
        [HttpPut("{id}/unblock")]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!user.IsBlocked)
            {
                return BadRequest("User is not blocked.");
            }

            user.IsBlocked = false;
            var success = await _userRepository.UpdateAsync(user);

            if (!success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to unblock user.");
            }

            return Ok(new { message = "User unblocked successfully." });
        }
    }
}