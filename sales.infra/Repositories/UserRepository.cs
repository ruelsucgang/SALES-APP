using Microsoft.EntityFrameworkCore;
using sales.domain.Entities;
using sales.infra.Data;
using sales.infra.Interfaces;

namespace sales.infra.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SalesDbContext _context;

        public UserRepository(SalesDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<IEnumerable<User>> GetAllAdminsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == "Admin")
                .ToListAsync();
        }
    }
}