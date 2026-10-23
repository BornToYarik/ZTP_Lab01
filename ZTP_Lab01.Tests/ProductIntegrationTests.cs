using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Xunit;
using ZTP_Lab01.Controllers;
using ZTP_Lab01.Domain.Enums;
using ZTP_Lab01.Dtos;
using ZTP_Lab01.Infarstructure;

namespace ZTP_Lab01.Tests;

public class ProductsIntegrationTests
{
    private static IProductsApi CreateApiClient()
    {
        var dbName = Guid.NewGuid().ToString();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services
                        .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                                    d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true)
                        .ToList();

                    foreach (var d in descriptors)
                        services.Remove(d);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase(dbName));
                });
            });

        var httpClient = factory.CreateClient();
        return RestService.For<IProductsApi>(httpClient);
    }

    // Достаем JSON-строку ошибки из ответа Refit
    private static string GetErrorContent<T>(ApiResponse<T> response) =>
        (response.Error as ApiException)?.Content ?? string.Empty;

    [Fact]
    public async Task FullCrud_And_HistoryTracking_ShouldWorkCorrectly()
    {
        var api = CreateApiClient();

        // 1. POST - создаем продукт
        var createRes = await api.CreateProductAsync(new CreateProductDto
        {
            Name = "PhoneX10",
            Category = ProductCategory.Electronics,
            Price = 2000m,
            StockQuantity = 15
        });
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var productId = createRes.Content!.Id;

        // 2. PUT - обновляем цену и количество (должна записаться история!)
        var updateRes = await api.UpdateProductAsync(productId, new UpdateProductDto
        {
            Name = "PhoneX10",
            Category = ProductCategory.Electronics,
            Price = 1800m,
            StockQuantity = 10
        });
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        // 3. GET /{id} - проверяем детали и историю изменений
        var detailsRes = await api.GetProductByIdAsync(productId);
        Assert.Equal(HttpStatusCode.OK, detailsRes.StatusCode);
        Assert.Equal(1800m, detailsRes.Content!.Price);
        Assert.Contains(detailsRes.Content.History, h => h.PropertyName == "Price" && h.OldValue == "2000" && h.NewValue == "1800");
        Assert.Contains(detailsRes.Content.History, h => h.PropertyName == "StockQuantity" && h.OldValue == "15" && h.NewValue == "10");

        // 4. DELETE - удаляем продукт
        var deleteRes = await api.DeleteProductAsync(productId);
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // 5. Проверяем, что после удаления возвращает 404 NotFound
        var notFoundRes = await api.GetProductByIdAsync(productId);
        Assert.Equal(HttpStatusCode.NotFound, notFoundRes.StatusCode);
    }

    [Theory]
    [InlineData("Ab")] // Слишком короткое (< 3)
    [InlineData("VeryLongProductNameExceeding20Chars")] // Слишком длинное (> 20)
    [InlineData("Laptop_Pro!")] // Запрещенные спецсимволы
    public async Task InvalidProductName_ShouldReturnBadRequest_WithNameFieldError(string invalidName)
    {
        var api = CreateApiClient();

        var response = await api.CreateProductAsync(new CreateProductDto
        {
            Name = invalidName,
            Category = ProductCategory.Books,
            Price = 50m,
            StockQuantity = 5
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Name", GetErrorContent(response));
    }

    [Fact]
    public async Task DuplicateProductName_ShouldReturnBadRequest()
    {
        var api = CreateApiClient();

        var dto = new CreateProductDto
        {
            Name = "UniqueBook1",
            Category = ProductCategory.Books,
            Price = 40m,
            StockQuantity = 3
        };

        var first = await api.CreateProductAsync(dto);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Вторая попытка с тем же именем
        var duplicate = await api.CreateProductAsync(dto);
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Contains("Name", GetErrorContent(duplicate));
    }

    [Theory]
    [InlineData(ProductCategory.Electronics, 49.99)]    // Мин 50
    [InlineData(ProductCategory.Electronics, 50000.01)] // Макс 50000
    [InlineData(ProductCategory.Books, 4.99)]           // Мин 5
    [InlineData(ProductCategory.Books, 500.01)]         // Макс 500
    [InlineData(ProductCategory.Clothing, 9.99)]        // Мин 10
    [InlineData(ProductCategory.Clothing, 5000.01)]     // Макс 5000
    public async Task PriceOutOfRangeForCategory_ShouldReturnBadRequest_WithPriceFieldError(ProductCategory category, decimal invalidPrice)
    {
        var api = CreateApiClient();

        var response = await api.CreateProductAsync(new CreateProductDto
        {
            Name = "ValidItem1",
            Category = category,
            Price = invalidPrice,
            StockQuantity = 10
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Price", GetErrorContent(response));
    }

    [Fact]
    public async Task NegativeStockQuantity_ShouldReturnBadRequest_WithStockQuantityFieldError()
    {
        var api = CreateApiClient();

        var response = await api.CreateProductAsync(new CreateProductDto
        {
            Name = "TShirtBlue",
            Category = ProductCategory.Clothing,
            Price = 100m,
            StockQuantity = -1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("StockQuantity", GetErrorContent(response));
    }

    [Fact]
    public async Task ForbiddenPhraseInName_ShouldBlockProductCreationAndUpdate()
    {
        var api = CreateApiClient();

        // 1. Динамически добавляем запрещенное слово "Scam" через API
        var phraseRes = await api.AddForbiddenPhraseAsync(new CreateForbiddenPhraseDto("Scam"));
        Assert.Equal(HttpStatusCode.Created, phraseRes.StatusCode);

        // 2. Пытаемся создать продукт с этим словом в названии
        var blockedCreate = await api.CreateProductAsync(new CreateProductDto
        {
            Name = "SuperScamItem",
            Category = ProductCategory.Electronics,
            Price = 100m,
            StockQuantity = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, blockedCreate.StatusCode);
        Assert.Contains("Name", GetErrorContent(blockedCreate));
    }
}