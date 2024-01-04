using sales.domain.Entities;

namespace sales.infra.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllAdminsAsync(); // For SuperAdmin to view all Admins
    }
}