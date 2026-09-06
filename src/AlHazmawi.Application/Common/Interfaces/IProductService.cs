using AlHazmawi.Application.DTOs.Catalog;

namespace AlHazmawi.Application.Common.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsByBusinessAsync(Guid businessId, Guid? categoryId = null);
    Task<ProductDto?> GetProductByIdAsync(Guid productId);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, string userId);
    Task<ProductDto?> UpdateProductAsync(Guid productId, UpdateProductRequest request, string userId);
    Task<bool> DeleteProductAsync(Guid productId, string userId);
    Task<bool> ToggleProductAvailabilityAsync(Guid productId, string userId);
    
    // Product categories (sections inside a store)
    Task<List<ProductCategoryDto>> GetProductCategoriesAsync(Guid businessId);
    Task<ProductCategoryDto> CreateProductCategoryAsync(CreateProductCategoryRequest request, string userId);
    Task<bool> DeleteProductCategoryAsync(Guid categoryId, string userId);
}
