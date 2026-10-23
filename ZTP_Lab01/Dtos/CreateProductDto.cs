using ZTP_Lab01.Domain.Enums;

namespace ZTP_Lab01.Dtos;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
