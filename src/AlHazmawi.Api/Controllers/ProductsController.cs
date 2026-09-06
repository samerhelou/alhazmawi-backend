using System.Security.Claims;
using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlHazmawi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets all products for a specific store, optionally filtered by store section.
    /// </summary>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBusiness(Guid businessId, [FromQuery] Guid? categoryId = null)
    {
        var products = await _productService.GetProductsByBusinessAsync(businessId, categoryId);
        return Ok(products);
    }

    /// <summary>
    /// Gets product details by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound("Product not found");
        return Ok(product);
    }

    /// <summary>
    /// Adds a new product to store (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var product = await _productService.CreateProductAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Updates product details and options (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var updated = await _productService.UpdateProductAsync(id, request, userId);
        if (updated == null) return NotFound("Product not found or not authorized");

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a product (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _productService.DeleteProductAsync(id, userId);
        if (!success) return NotFound("Product not found or not authorized");

        return NoContent();
    }

    /// <summary>
    /// Toggles product availability / out-of-stock (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpPatch("{id:guid}/toggle-availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleAvailability(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var isAvailable = await _productService.ToggleProductAvailabilityAsync(id, userId);
        return Ok(new { IsAvailable = isAvailable });
    }

    /// <summary>
    /// Gets in-store menu categories / sections.
    /// </summary>
    [HttpGet("categories/{businessId:guid}")]
    [ProducesResponseType(typeof(List<ProductCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(Guid businessId)
    {
        var categories = await _productService.GetProductCategoriesAsync(businessId);
        return Ok(categories);
    }

    /// <summary>
    /// Adds a new section / category to store menu (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpPost("categories")]
    [ProducesResponseType(typeof(ProductCategoryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateProductCategoryRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var category = await _productService.CreateProductCategoryAsync(request, userId);
            return Ok(category);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a store menu category (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpDelete("categories/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _productService.DeleteProductCategoryAsync(id, userId);
        if (!success) return NotFound("Category not found or not authorized");

        return NoContent();
    }
}
