using System;

namespace ZTP_Lab01.Domains.Entities;

public class ProductHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string? oldValue {get; set;}
    public string? newValue {get; set;}
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}