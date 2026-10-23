using Refit;
using ZTP_Lab01.Controllers;
using ZTP_Lab01.Domains.Entities;
using ZTP_Lab01.Dtos;

namespace ZTP_Lab01.Tests;

public interface IProductsApi
{
    [Get("/api/v1/products")]
    Task<ApiResponse<List<ProductListItemDto>>> GetAllProductsAsync();

    [Get("/api/v1/products/{id}")]
    Task<ApiResponse<ProductDetailsDto>> GetProductByIdAsync(int id);

    [Post("/api/v1/products")]
    Task<ApiResponse<ProductDetailsDto>> CreateProductAsync([Body] CreateProductDto dto);

    [Put("/api/v1/products/{id}")]
    Task<ApiResponse<ProductDetailsDto>> UpdateProductAsync(int id, [Body] UpdateProductDto dto);

    [Delete("/api/v1/products/{id}")]
    Task<HttpResponseMessage> DeleteProductAsync(int id);

    [Post("/api/v1/forbidden-phrases")]
    Task<ApiResponse<ForbiddenPhrase>> AddForbiddenPhraseAsync([Body] CreateForbiddenPhraseDto dto);
}