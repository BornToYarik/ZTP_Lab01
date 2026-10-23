using System;
using System.Collections.Generic;
using ZTP_Lab01.Domain.Enums;

namespace ZTP_Lab01.Dtos;

public class ProductListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class ProductDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public string CategoryName => Category.ToString();
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public List<ProductHistoryDto> History { get; set; } = new();
}

public class ProductHistoryDto
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
}