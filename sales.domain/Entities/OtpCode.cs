using sales.domain.Entities;

namespace sales.domain
{
    public class OtpCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        // Navigation property
        public User? User { get; set; }
    }
}