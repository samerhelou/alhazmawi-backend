using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Catalog;
using AlHazmawi.Domain.Entities;
using AlHazmawi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlHazmawi.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AlHazmawiDbContext _context;

    public CategoryService(AlHazmawiDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(bool onlyActive = true)
    {
        var query = _context.BusinessCategories.AsNoTracking();

        if (onlyActive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(
                c.Id,
                c.NameArabic,
                c.NameEnglish,
                c.IconUrl,
                c.IconEmoji,
                c.SortOrder,
                c.Businesses.Count(b => b.IsActive)
            ))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var c = await _context.BusinessCategories
            .AsNoTracking()
            .Include(x => x.Businesses)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (c == null) return null;

        return new CategoryDto(
            c.Id,
            c.NameArabic,
            c.NameEnglish,
            c.IconUrl,
            c.IconEmoji,
            c.SortOrder,
            c.Businesses.Count(b => b.IsActive)
        );
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new BusinessCategory
        {
            NameArabic = request.NameArabic.Trim(),
            NameEnglish = request.NameEnglish.Trim(),
            IconUrl = request.IconUrl,
            IconEmoji = request.IconEmoji,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _context.BusinessCategories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto(
            category.Id,
            category.NameArabic,
            category.NameEnglish,
            category.IconUrl,
            category.IconEmoji,
            category.SortOrder,
            0
        );
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _context.BusinessCategories.FindAsync(id);
        if (category == null) return null;

        category.NameArabic = request.NameArabic.Trim();
        category.NameEnglish = request.NameEnglish.Trim();
        category.IconUrl = request.IconUrl;
        category.IconEmoji = request.IconEmoji;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        var count = await _context.Businesses.CountAsync(b => b.CategoryId == id && b.IsActive);
        return new CategoryDto(
            category.Id,
            category.NameArabic,
            category.NameEnglish,
            category.IconUrl,
            category.IconEmoji,
            category.SortOrder,
            count
        );
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.BusinessCategories.FindAsync(id);
        if (category == null) return false;

        // Soft delete
        category.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
