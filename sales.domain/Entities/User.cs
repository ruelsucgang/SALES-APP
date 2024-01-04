using System;

namespace sales.domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!; // SuperAdmin, Admin, Customer
        public bool IsApproved { get; set; } = false; // Admin needs SuperAdmin approval
        public bool IsBlocked { get; set; } = false; // Can be blocked by SuperAdmin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}