using System.Collections.Generic;
using System.Threading.Tasks;
using ZTP_Lab01.Dtos;

namespace ZTP_Lab01.Services;

public interface IProductService
{
    Task<List<ProductListItemDto>> GetAllAsync();
    Task<ProductDetailsDto?> GetByIdAsync(int id);
    Task<ProductDetailsDto> CreateAsync(CreateProductDto dto);
    Task<ProductDetailsDto?> UpdateAsync(int id, UpdateProductDto dto);
    Task<bool> DeleteAsync(int id);
}