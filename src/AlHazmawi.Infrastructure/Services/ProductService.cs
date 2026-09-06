using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Catalog;
using AlHazmawi.Domain.Entities;
using AlHazmawi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlHazmawi.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AlHazmawiDbContext _context;

    public ProductService(AlHazmawiDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetProductsByBusinessAsync(Guid businessId, Guid? categoryId = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.ProductCategory)
            .Include(p => p.Options.Where(o => o.IsActive))
            .Where(p => p.BusinessId == businessId);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategoryId == categoryId.Value);
        }

        var products = await query.OrderBy(p => p.SortOrder).ToListAsync();
        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.ProductCategory)
            .Include(p => p.Options.Where(o => o.IsActive))
            .FirstOrDefaultAsync(p => p.Id == productId);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, string userId)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == request.BusinessId);
        if (business == null || business.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to add products to this business.");

        var product = new Product
        {
            BusinessId = request.BusinessId,
            ProductCategoryId = request.ProductCategoryId,
            NameArabic = request.NameArabic.Trim(),
            NameEnglish = request.NameEnglish.Trim(),
            DescriptionArabic = request.DescriptionArabic,
            DescriptionEnglish = request.DescriptionEnglish,
            Price = request.Price,
            ImageUrl = request.ImageUrl,
            SortOrder = request.SortOrder,
            IsAvailable = true
        };

        if (request.Options != null && request.Options.Any())
        {
            foreach (var opt in request.Options)
            {
                product.Options.Add(new ProductOption
                {
                    NameArabic = opt.NameArabic.Trim(),
                    NameEnglish = opt.NameEnglish.Trim(),
                    AdditionalPrice = opt.AdditionalPrice,
                    IsRequired = opt.IsRequired,
                    IsActive = true
                });
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateProductAsync(Guid productId, UpdateProductRequest request, string userId)
    {
        var product = await _context.Products
            .Include(p => p.Business)
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null || product.Business.UserId != userId) return null;

        product.ProductCategoryId = request.ProductCategoryId;
        product.NameArabic = request.NameArabic.Trim();
        product.NameEnglish = request.NameEnglish.Trim();
        product.DescriptionArabic = request.DescriptionArabic;
        product.DescriptionEnglish = request.DescriptionEnglish;
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        product.IsAvailable = request.IsAvailable;
        product.SortOrder = request.SortOrder;

        // Update options if provided
        if (request.Options != null)
        {
            // Remove existing options
            _context.ProductOptions.RemoveRange(product.Options);
            
            // Add updated options
            foreach (var opt in request.Options)
            {
                product.Options.Add(new ProductOption
                {
                    ProductId = product.Id,
                    NameArabic = opt.NameArabic.Trim(),
                    NameEnglish = opt.NameEnglish.Trim(),
                    AdditionalPrice = opt.AdditionalPrice,
                    IsRequired = opt.IsRequired,
                    IsActive = true
                });
            }
        }

        await _context.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task<bool> DeleteProductAsync(Guid productId, string userId)
    {
        var product = await _context.Products
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null || product.Business.UserId != userId) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleProductAvailabilityAsync(Guid productId, string userId)
    {
        var product = await _context.Products
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null || product.Business.UserId != userId) return false;

        product.IsAvailable = !product.IsAvailable;
        await _context.SaveChangesAsync();
        return product.IsAvailable;
    }

    public async Task<List<ProductCategoryDto>> GetProductCategoriesAsync(Guid businessId)
    {
        return await _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new ProductCategoryDto(
                c.Id,
                c.BusinessId,
                c.NameArabic,
                c.NameEnglish,
                c.SortOrder,
                c.Products.Count
            ))
            .ToListAsync();
    }

    public async Task<ProductCategoryDto> CreateProductCategoryAsync(CreateProductCategoryRequest request, string userId)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == request.BusinessId);
        if (business == null || business.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to add categories to this business.");

        var category = new ProductCategory
        {
            BusinessId = request.BusinessId,
            NameArabic = request.NameArabic.Trim(),
            NameEnglish = request.NameEnglish.Trim(),
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return new ProductCategoryDto(category.Id, category.BusinessId, category.NameArabic, category.NameEnglish, category.SortOrder, 0);
    }

    public async Task<bool> DeleteProductCategoryAsync(Guid categoryId, string userId)
    {
        var category = await _context.ProductCategories
            .Include(c => c.Business)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null || category.Business.UserId != userId) return false;

        category.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToDto(Product p)
    {
        return new ProductDto(
            p.Id,
            p.BusinessId,
            p.ProductCategoryId,
            p.ProductCategory?.NameArabic,
            p.ProductCategory?.NameEnglish,
            p.NameArabic,
            p.NameEnglish,
            p.DescriptionArabic,
            p.DescriptionEnglish,
            p.Price,
            p.ImageUrl,
            p.IsAvailable,
            p.SortOrder,
            p.Options.Select(o => new ProductOptionDto(
                o.Id,
                o.NameArabic,
                o.NameEnglish,
                o.AdditionalPrice,
                o.IsRequired
            )).ToList()
        );
    }
}
