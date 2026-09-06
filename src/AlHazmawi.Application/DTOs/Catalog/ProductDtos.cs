namespace AlHazmawi.Application.DTOs.Catalog;

public record ProductOptionDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    decimal AdditionalPrice,
    bool IsRequired
);

public record ProductDto(
    Guid Id,
    Guid BusinessId,
    Guid? ProductCategoryId,
    string? CategoryNameArabic,
    string? CategoryNameEnglish,
    string NameArabic,
    string NameEnglish,
    string? DescriptionArabic,
    string? DescriptionEnglish,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int SortOrder,
    List<ProductOptionDto> Options
);

public record CreateProductOptionRequest(
    string NameArabic,
    string NameEnglish,
    decimal AdditionalPrice,
    bool IsRequired
);

public record CreateProductRequest(
    Guid BusinessId,
    Guid? ProductCategoryId,
    string NameArabic,
    string NameEnglish,
    string? DescriptionArabic,
    string? DescriptionEnglish,
    decimal Price,
    string? ImageUrl,
    int SortOrder = 0,
    List<CreateProductOptionRequest>? Options = null
);

public record UpdateProductRequest(
    Guid? ProductCategoryId,
    string NameArabic,
    string NameEnglish,
    string? DescriptionArabic,
    string? DescriptionEnglish,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    int SortOrder,
    List<CreateProductOptionRequest>? Options = null
);

public record CreateProductCategoryRequest(
    Guid BusinessId,
    string NameArabic,
    string NameEnglish,
    int SortOrder = 0
);

public record ProductCategoryDto(
    Guid Id,
    Guid BusinessId,
    string NameArabic,
    string NameEnglish,
    int SortOrder,
    int ProductCount
);
