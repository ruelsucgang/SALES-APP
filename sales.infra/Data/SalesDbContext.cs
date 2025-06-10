using Microsoft.EntityFrameworkCore;
using sales.domain;
using sales.domain.Entities;

namespace sales.infra.Data
{
    public class SalesDbContext : DbContext
    {
        public SalesDbContext(DbContextOptions<SalesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<OtpCode> OtpCodes { get; set; } = null!;   
        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.FullName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.BillingAddress)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(c => c.ContactNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.CreatedAt)
                    .IsRequired();

                entity.Property(c => c.UpdatedAt)
                    .IsRequired(false);

                // Foreign key to User
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index for faster lookups
                entity.HasIndex(c => c.UserId);
            });

            // OtpCode configuration
            modelBuilder.Entity<OtpCode>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(o => o.Code)
                    .IsRequired()
                    .HasMaxLength(6);

                entity.Property(o => o.ExpiresAt)
                    .IsRequired();

                entity.Property(o => o.IsUsed)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(o => o.CreatedAt)
                    .IsRequired();

                // Foreign key to User
                entity.HasOne(o => o.User)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for faster OTP validation queries
                entity.HasIndex(o => o.UserId);
                entity.HasIndex(o => new { o.Email, o.IsUsed, o.ExpiresAt });
            });
        }
    }
}
