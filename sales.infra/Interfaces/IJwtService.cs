using sales.domain.Entities;

namespace sales.infra.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}