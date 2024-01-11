using BCrypt.Net;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sales.domain.Entities;
using sales.infra.DTOs;
using sales.infra.Interfaces;

namespace sales.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;

        public AuthController(
            IUserRepository userRepository,
            IJwtService jwtService,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            // Validate input
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return ValidationProblem(ModelState);
            }

            // Check if username already exists
            var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUsername != null)
            {
                return BadRequest("Username already exists.");
            }

            // Check if email already exists
            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
            {
                return BadRequest("Email already exists.");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create new user
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role,
                IsApproved = dto.Role == "Customer", // Customer auto-approved, Admin needs approval
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);

            var result = new UserDto
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                Email = createdUser.Email,
                Role = createdUser.Role,
                IsApproved = createdUser.IsApproved,
                IsBlocked = createdUser.IsBlocked,
                CreatedAt = createdUser.CreatedAt
            };

            return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            // Validate input
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                return ValidationProblem(ModelState);
            }

            // Find user by username
            var user = await _userRepository.GetByUsernameAsync(dto.Username);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Verify password
            var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                return Unauthorized("Invalid username or password.");
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                return Unauthorized("Your account has been blocked. Please contact support.");
            }

            // Check if Admin is approved
            if (user.Role == "Admin" && !user.IsApproved)
            {
                return Unauthorized("Your Admin account is pending approval.");
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // In JWT, logout is handled client-side by deleting the token
            // This endpoint is just for logging/tracking purposes

            var username = User.Identity?.Name;

            return Ok(new { message = "Logged out successfully. Please delete your token on the client side." });
        }
    }
}