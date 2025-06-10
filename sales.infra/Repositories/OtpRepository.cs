using Microsoft.EntityFrameworkCore;
using sales.domain;
using sales.domain.Entities;
using sales.infra.Data;
using sales.infra.Interfaces;

namespace sales.infra.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly SalesDbContext _context;

        public OtpRepository(SalesDbContext context)
        {
            _context = context;
        }

        public async Task<OtpCode> CreateAsync(OtpCode otpCode)
        {
            _context.OtpCodes.Add(otpCode);
            await _context.SaveChangesAsync();
            return otpCode;
        }

        public async Task<OtpCode?> GetValidOtpAsync(string email, string code)
        {
            return await _context.OtpCodes
                .Where(o => o.Email == email
                    && o.Code == code
                    && !o.IsUsed
                    && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task MarkAsUsedAsync(int otpId)
        {
            var otp = await _context.OtpCodes.FindAsync(otpId);
            if (otp != null)
            {
                otp.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteExpiredOtpsAsync()
        {
            var expiredOtps = await _context.OtpCodes
                .Where(o => o.ExpiresAt < DateTime.UtcNow || o.IsUsed)
                .ToListAsync();

            _context.OtpCodes.RemoveRange(expiredOtps);
            await _context.SaveChangesAsync();
        }
    }
}