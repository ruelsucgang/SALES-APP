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
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

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

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.OrderDate)
                    .IsRequired();

                entity.Property(o => o.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(o => o.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");

                entity.Property(o => o.PaymentStatus)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Unpaid");

                entity.Property(o => o.PaymentIntentId)
                    .HasMaxLength(255);

                entity.Property(o => o.CreatedAt)
                    .IsRequired();

                entity.Property(o => o.UpdatedAt)
                    .IsRequired(false);

                // Foreign key to Customer
                entity.HasOne(o => o.Customer)
                    .WithMany()
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Index for faster lookups
                entity.HasIndex(o => o.CustomerId);
                entity.HasIndex(o => o.OrderDate);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.Property(oi => oi.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(oi => oi.UnitPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.Quantity)
                    .IsRequired();

                entity.Property(oi => oi.TotalPrice)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                // Foreign key to Order
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Foreign key to Product
                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(oi => oi.OrderId);
                entity.HasIndex(oi => oi.ProductId);
            });
        }
    }
}
