using System.Collections.Generic;
using ZTP_Lab01.Domain.Enums;

namespace ZTP_Lab01.Domains.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    public List<ProductHistory> History { get; set; } = new();
}