namespace AlHazmawi.Application.DTOs.Business;

public record BusinessDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    string? DescriptionArabic,
    string? DescriptionEnglish,
    string Phone,
    string AddressArabic,
    string? AddressEnglish,
    double Latitude,
    double Longitude,
    string? LogoUrl,
    string? CoverUrl,
    decimal DeliveryFee,
    decimal? MinimumOrderAmount,
    int EstimatedDeliveryMinutes,
    double AverageRating,
    int ReviewCount,
    Guid CategoryId,
    string? CategoryNameArabic,
    string? CategoryNameEnglish,
    bool IsOpen,
    double? DistanceKm = null
);

public record WorkingHoursDto(
    DayOfWeek DayOfWeek,
    TimeOnly? OpeningTime,
    TimeOnly? ClosingTime,
    bool IsOpen
);

public record BusinessDetailDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    string? DescriptionArabic,
    string? DescriptionEnglish,
    string Phone,
    string AddressArabic,
    string? AddressEnglish,
    double Latitude,
    double Longitude,
    string? LogoUrl,
    string? CoverUrl,
    decimal DeliveryFee,
    decimal? MinimumOrderAmount,
    int EstimatedDeliveryMinutes,
    double AverageRating,
    int ReviewCount,
    Guid CategoryId,
    string? CategoryNameArabic,
    string? CategoryNameEnglish,
    bool IsOpen,
    List<WorkingHoursDto> WorkingHours,
    List<StoreSectionDto> MenuSections
);

public record StoreSectionDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    int SortOrder,
    List<Catalog.ProductDto> Products
);

public record BusinessQueryParameters(
    Guid? CategoryId = null,
    string? SearchTerm = null,
    double? Latitude = null,
    double? Longitude = null,
    int PageNumber = 1,
    int PageSize = 20
);

public record UpdateBusinessProfileRequest(
    string NameArabic,
    string NameEnglish,
    string? DescriptionArabic,
    string? DescriptionEnglish,
    string Phone,
    string AddressArabic,
    string? AddressEnglish,
    double Latitude,
    double Longitude,
    string? LogoUrl,
    string? CoverUrl,
    decimal DeliveryFee,
    decimal? MinimumOrderAmount,
    int EstimatedDeliveryMinutes
);
