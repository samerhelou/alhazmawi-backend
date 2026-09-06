using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Business;
using AlHazmawi.Application.DTOs.Catalog;
using AlHazmawi.Domain.Entities;
using AlHazmawi.Domain.Enums;
using AlHazmawi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AlHazmawi.Infrastructure.Services;

public class BusinessService : IBusinessService
{
    private readonly AlHazmawiDbContext _context;

    public BusinessService(AlHazmawiDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusinessDto>> GetBusinessesAsync(BusinessQueryParameters parameters)
    {
        var query = _context.Businesses
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.WorkingHours)
            .Where(b => b.IsActive && b.Status == BusinessStatus.Approved);

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == parameters.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var search = parameters.SearchTerm.Trim().ToLower();
            query = query.Where(b =>
                b.NameArabic.ToLower().Contains(search) ||
                b.NameEnglish.ToLower().Contains(search));
        }

        var businesses = await query
            .OrderByDescending(b => b.AverageRating)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var currentDay = now.DayOfWeek;
        var currentTime = TimeOnly.FromDateTime(now);

        return businesses.Select(b =>
        {
            double? distance = null;
            if (parameters.Latitude.HasValue && parameters.Longitude.HasValue)
            {
                distance = CalculateDistanceKm(
                    parameters.Latitude.Value,
                    parameters.Longitude.Value,
                    b.Latitude,
                    b.Longitude);
            }

            var todayHours = b.WorkingHours.FirstOrDefault(h => h.DayOfWeek == currentDay);
            var isOpen = todayHours != null && todayHours.IsOpen &&
                         todayHours.OpeningTime.HasValue && todayHours.ClosingTime.HasValue &&
                         currentTime >= todayHours.OpeningTime.Value &&
                         currentTime <= todayHours.ClosingTime.Value;

            return new BusinessDto(
                b.Id,
                b.NameArabic,
                b.NameEnglish,
                b.DescriptionArabic,
                b.DescriptionEnglish,
                b.Phone,
                b.AddressArabic,
                b.AddressEnglish,
                b.Latitude,
                b.Longitude,
                b.LogoUrl,
                b.CoverUrl,
                b.DeliveryFee,
                b.MinimumOrderAmount,
                b.EstimatedDeliveryMinutes,
                b.AverageRating,
                b.ReviewCount,
                b.CategoryId,
                b.Category?.NameArabic,
                b.Category?.NameEnglish,
                isOpen,
                distance
            );
        }).ToList();
    }

    public async Task<BusinessDetailDto?> GetBusinessDetailsAsync(Guid id)
    {
        var business = await _context.Businesses
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.WorkingHours)
            .Include(b => b.Products.Where(p => p.IsAvailable))
                .ThenInclude(p => p.Options.Where(o => o.IsActive))
            .FirstOrDefaultAsync(b => b.Id == id);

        if (business == null) return null;

        var productCategories = await _context.ProductCategories
            .AsNoTracking()
            .Where(c => c.BusinessId == id && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var currentTime = TimeOnly.FromDateTime(now);
        var todayHours = business.WorkingHours.FirstOrDefault(h => h.DayOfWeek == now.DayOfWeek);
        var isOpen = todayHours != null && todayHours.IsOpen &&
                     todayHours.OpeningTime.HasValue && todayHours.ClosingTime.HasValue &&
                     currentTime >= todayHours.OpeningTime.Value &&
                     currentTime <= todayHours.ClosingTime.Value;

        var workingHoursDto = business.WorkingHours
            .OrderBy(h => h.DayOfWeek)
            .Select(h => new WorkingHoursDto(h.DayOfWeek, h.OpeningTime, h.ClosingTime, h.IsOpen))
            .ToList();

        // Group products by ProductCategory
        var sections = new List<StoreSectionDto>();

        foreach (var cat in productCategories)
        {
            var categoryProducts = business.Products
                .Where(p => p.ProductCategoryId == cat.Id)
                .OrderBy(p => p.SortOrder)
                .Select(p => MapToProductDto(p, business.NameArabic, business.NameEnglish))
                .ToList();

            if (categoryProducts.Any())
            {
                sections.Add(new StoreSectionDto(cat.Id, cat.NameArabic, cat.NameEnglish, cat.SortOrder, categoryProducts));
            }
        }

        // Uncategorized products
        var uncategorized = business.Products
            .Where(p => p.ProductCategoryId == null)
            .OrderBy(p => p.SortOrder)
            .Select(p => MapToProductDto(p, business.NameArabic, business.NameEnglish))
            .ToList();

        if (uncategorized.Any())
        {
            sections.Add(new StoreSectionDto(Guid.Empty, "أخرى", "Other", 999, uncategorized));
        }

        return new BusinessDetailDto(
            business.Id,
            business.NameArabic,
            business.NameEnglish,
            business.DescriptionArabic,
            business.DescriptionEnglish,
            business.Phone,
            business.AddressArabic,
            business.AddressEnglish,
            business.Latitude,
            business.Longitude,
            business.LogoUrl,
            business.CoverUrl,
            business.DeliveryFee,
            business.MinimumOrderAmount,
            business.EstimatedDeliveryMinutes,
            business.AverageRating,
            business.ReviewCount,
            business.CategoryId,
            business.Category?.NameArabic,
            business.Category?.NameEnglish,
            isOpen,
            workingHoursDto,
            sections
        );
    }

    public async Task<BusinessDto?> GetBusinessByUserIdAsync(string userId)
    {
        var b = await _context.Businesses
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (b == null) return null;

        return new BusinessDto(
            b.Id,
            b.NameArabic,
            b.NameEnglish,
            b.DescriptionArabic,
            b.DescriptionEnglish,
            b.Phone,
            b.AddressArabic,
            b.AddressEnglish,
            b.Latitude,
            b.Longitude,
            b.LogoUrl,
            b.CoverUrl,
            b.DeliveryFee,
            b.MinimumOrderAmount,
            b.EstimatedDeliveryMinutes,
            b.AverageRating,
            b.ReviewCount,
            b.CategoryId,
            b.Category?.NameArabic,
            b.Category?.NameEnglish,
            b.IsActive
        );
    }

    public async Task<bool> UpdateBusinessProfileAsync(Guid businessId, UpdateBusinessProfileRequest request, string userId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null || business.UserId != userId) return false;

        business.NameArabic = request.NameArabic;
        business.NameEnglish = request.NameEnglish;
        business.DescriptionArabic = request.DescriptionArabic;
        business.DescriptionEnglish = request.DescriptionEnglish;
        business.Phone = request.Phone;
        business.AddressArabic = request.AddressArabic;
        business.AddressEnglish = request.AddressEnglish;
        business.Latitude = request.Latitude;
        business.Longitude = request.Longitude;
        business.LogoUrl = request.LogoUrl;
        business.CoverUrl = request.CoverUrl;
        business.DeliveryFee = request.DeliveryFee;
        business.MinimumOrderAmount = request.MinimumOrderAmount;
        business.EstimatedDeliveryMinutes = request.EstimatedDeliveryMinutes;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleBusinessOpenStatusAsync(Guid businessId, string userId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null || business.UserId != userId) return false;

        business.IsActive = !business.IsActive;
        await _context.SaveChangesAsync();
        return business.IsActive;
    }

    public async Task<List<BusinessDto>> GetPendingBusinessesAsync()
    {
        var list = await _context.Businesses
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.Status == BusinessStatus.PendingApproval)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return list.Select(b => new BusinessDto(
            b.Id, b.NameArabic, b.NameEnglish, b.DescriptionArabic, b.DescriptionEnglish,
            b.Phone, b.AddressArabic, b.AddressEnglish, b.Latitude, b.Longitude,
            b.LogoUrl, b.CoverUrl, b.DeliveryFee, b.MinimumOrderAmount,
            b.EstimatedDeliveryMinutes, b.AverageRating, b.ReviewCount,
            b.CategoryId, b.Category?.NameArabic, b.Category?.NameEnglish, b.IsActive
        )).ToList();
    }

    public async Task<bool> ApproveBusinessAsync(Guid businessId)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null) return false;
        business.Status = BusinessStatus.Approved;
        business.IsActive = true;
        business.ApprovedAt = DateTime.UtcNow;
        business.RejectionReason = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectBusinessAsync(Guid businessId, string? reason)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null) return false;
        business.Status = BusinessStatus.Rejected;
        business.IsActive = false;
        business.RejectionReason = reason;
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToProductDto(Product p, string businessNameAr, string businessNameEn)
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

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var r = 6371; // Earth radius in KM
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(r * c, 2);
    }
}
