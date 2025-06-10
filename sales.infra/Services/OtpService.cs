using sales.domain;
using sales.domain.Entities;
using sales.infra.Interfaces;

namespace sales.infra.Services
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;

        public OtpService(
            IOtpRepository otpRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            IJwtService jwtService)
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        public async Task<bool> RequestOtpAsync(string email)
        {
            // Find user with email and role "Customer"
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.Role != "Customer")
            {
                return false; // Email not found or not a customer
            }

            // Check if user is blocked
            if (user.IsBlocked)
            {
                throw new InvalidOperationException("Your account has been blocked.");
            }

            // Generate OTP code
            var otpCode = GenerateOtpCode();

            // Create OTP record
            var otp = new OtpCode
            {
                UserId = user.Id,
                Email = email,
                Code = otpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minutes expiration
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _otpRepository.CreateAsync(otp);

            // Send OTP via email
            await _emailService.SendOtpEmailAsync(email, otpCode);

            return true;
        }

        public async Task<string?> VerifyOtpAsync(string email, string code)
        {
            // Find valid OTP
            var otp = await _otpRepository.GetValidOtpAsync(email, code);
            if (otp == null)
            {
                return null; // Invalid or expired OTP
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(otp.UserId);
            if (user == null || user.IsBlocked)
            {
                return null; // User not found or blocked
            }

            // Mark OTP as used
            await _otpRepository.MarkAsUsedAsync(otp.Id);

            // Generate JWT token 
            var token = _jwtService.GenerateToken(user);

            return token;
        }

        public string GenerateOtpCode()
        {
            // Generate random 6-digit code (000000 - 999999)
            var random = new Random();
            var code = random.Next(0, 1000000).ToString("D6");
            return code;
        }
    }
}