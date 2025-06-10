namespace sales.infra.Interfaces
{
    public interface IOtpService
    {
        Task<bool> RequestOtpAsync(string email);
        Task<string?> VerifyOtpAsync(string email, string code);
        string GenerateOtpCode();
    }
}