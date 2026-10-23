using System.Collections.Generic;
using System.Threading.Tasks;
using ZTP_Lab01.Domains.Entities;

namespace ZTP_Lab01.Infarstructure.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdWithHistoryAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}