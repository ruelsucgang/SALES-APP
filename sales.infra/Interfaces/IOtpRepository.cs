using sales.domain;
using sales.domain.Entities;

namespace sales.infra.Interfaces
{
    public interface IOtpRepository
    {
        Task<OtpCode> CreateAsync(OtpCode otpCode);
        Task<OtpCode?> GetValidOtpAsync(string email, string code);
        Task MarkAsUsedAsync(int otpId);
        Task DeleteExpiredOtpsAsync();
    }
}