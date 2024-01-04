using Microsoft.EntityFrameworkCore;
using sales.domain.Entities;
using sales.infra.Data;
using sales.infra.Interfaces;

namespace sales.infra.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly SalesDbContext _context;
        public ProductRepository(SalesDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }
        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<bool> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Products.FindAsync(id);

            if (existing == null)
            {
                return false;
            }

            _context.Products.Remove(existing);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }
    }
}
