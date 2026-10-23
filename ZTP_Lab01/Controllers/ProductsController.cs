using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZTP_Lab01.Domain.Exceptions;
using ZTP_Lab01.Dtos;
using ZTP_Lab01.Services;

namespace ZTP_Lab01.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: /api/v1/products (krotki spis produktow)
    [HttpGet]
    public async Task<ActionResult<List<ProductListItemDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    // GET: /api/v1/products/{id} (produkt details)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailsDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product with ID {id} not found." });

        return Ok(product);
    }

    // POST: /api/v1/products 
    [HttpPost]
    public async Task<ActionResult<ProductDetailsDto>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var created = await _productService.CreateAsync(dto);
            // 201 Created
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    // PUT: /api/v1/products/{id} 
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDetailsDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var updated = await _productService.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(new { message = $"Product with ID {id} not found." });

            return Ok(updated);
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    // DELETE: /api/v1/products/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Product with ID {id} not found." });

        return NoContent(); // 204 No Content 
    }
}