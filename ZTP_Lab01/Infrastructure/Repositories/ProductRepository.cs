using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZTP_Lab01.Domains.Entities;

namespace ZTP_Lab01.Infarstructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _db.Products.AsNoTracking().ToListAsync();
    }

    public async Task<Product?> GetByIdWithHistoryAsync(int id)
    {
        return await _db.Products
            .Include(p => p.History.OrderByDescending(h => h.ChangedAt))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _db.Products.AnyAsync(p => 
            p.Name.ToLower() == name.ToLower() && 
            (!excludeId.HasValue || p.Id != excludeId.Value));
    }
    public async Task<Product> AddAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Product product)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
    }
}