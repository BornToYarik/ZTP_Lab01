using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZTP_Lab01.Domain.Enums;
using ZTP_Lab01.Domain.Exceptions;
using ZTP_Lab01.Domains.Entities;
using ZTP_Lab01.Dtos;
using ZTP_Lab01.Infarstructure.Repositories;

namespace ZTP_Lab01.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IForbiddenPhraseRepository _phraseRepo;

    public ProductService(IProductRepository productRepo, IForbiddenPhraseRepository phraseRepo)
    {
        _productRepo = productRepo;
        _phraseRepo = phraseRepo;
    }

    public async Task<List<ProductListItemDto>> GetAllAsync()
    {
        var products = await _productRepo.GetAllAsync();
        return products.Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        }).ToList();
    }

    public async Task<ProductDetailsDto?> GetByIdAsync(int id)
    {
        var product = await _productRepo.GetByIdWithHistoryAsync(id);
        return product is null ? null : MapToDetailsDto(product);
    }

    public async Task<ProductDetailsDto> CreateAsync(CreateProductDto dto)
    {
        await ValidateProductAsync(dto.Name, dto.Category, dto.Price, dto.StockQuantity);

        var product = new Product
        {
            Name = dto.Name,
            Category = dto.Category,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity
        };

        await _productRepo.AddAsync(product);
        return MapToDetailsDto(product);
    }

    public async Task<ProductDetailsDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepo.GetByIdWithHistoryAsync(id);
        if (product is null) return null;

        await ValidateProductAsync(dto.Name, dto.Category, dto.Price, dto.StockQuantity, excludeId: id);

        product.Name = dto.Name;
        product.Category = dto.Category;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;

        await _productRepo.UpdateAsync(product);
        
        var updated = await _productRepo.GetByIdWithHistoryAsync(id);
        return MapToDetailsDto(updated!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepo.GetByIdWithHistoryAsync(id);
        if (product is null) return false;

        await _productRepo.DeleteAsync(product);
        return true;
    }

    private async Task ValidateProductAsync(string name, ProductCategory category, decimal price, int stockQuantity, int? excludeId = null)
    {
        var errors = new Dictionary<string, List<string>>();

        void AddError(string field, string message)
        {
            if (!errors.ContainsKey(field))
                errors[field] = new List<string>();
            errors[field].Add(message);
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 20)
        {
            AddError(nameof(CreateProductDto.Name), "Product name must be between 3 and 20 characters long.");
        }

        if (!string.IsNullOrEmpty(name) && !Regex.IsMatch(name, "^[a-zA-Z0-9]+$"))
        {
            AddError(nameof(CreateProductDto.Name), "Product name can only contain letters and digits.");
        }

        if (!string.IsNullOrWhiteSpace(name) && await _productRepo.ExistsByNameAsync(name, excludeId))
        {
            AddError(nameof(CreateProductDto.Name), $"A product with the name '{name}' already exists.");
        }

        var forbiddenPhrases = await _phraseRepo.GetAllAsync();
        foreach (var fp in forbiddenPhrases)
        {
            if (!string.IsNullOrWhiteSpace(fp.Phrase) && 
                name.Contains(fp.Phrase, StringComparison.OrdinalIgnoreCase))
            {
                AddError(nameof(CreateProductDto.Name), $"Product name contains a forbidden phrase: '{fp.Phrase}'.");
            }
        }

        switch (category)
        {
            case ProductCategory.Electronics:
                if (price < 50m || price > 50_000m)
                    AddError(nameof(CreateProductDto.Price), "For Electronics, price must be between 50 and 50,000 PLN.");
                break;
            case ProductCategory.Books:
                if (price < 5m || price > 500m)
                    AddError(nameof(CreateProductDto.Price), "For Books, price must be between 5 and 500 PLN.");
                break;
            case ProductCategory.Clothing:
                if (price < 10m || price > 5_000m)
                    AddError(nameof(CreateProductDto.Price), "For Clothing, price must be between 10 and 5,000 PLN.");
                break;
            default:
                AddError(nameof(CreateProductDto.Category), "Invalid product category. Allowed values: 1 (Electronics), 2 (Books), 3 (Clothing).");
                break;
        }

        if (stockQuantity < 0)
        {
            AddError(nameof(CreateProductDto.StockQuantity), "Stock quantity cannot be negative (must be at least 0).");
        }

        if (errors.Count > 0)
        {
            throw new DomainValidationException(errors);
        }
    }

    private static ProductDetailsDto MapToDetailsDto(Product product)
    {
        return new ProductDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            History = product.History.Select(h => new ProductHistoryDto
            {
                PropertyName = h.PropertyName,
                OldValue = h.oldValue,
                NewValue = h.newValue,
                ChangedAt = h.ChangedAt
            }).ToList()
        };
    }
}