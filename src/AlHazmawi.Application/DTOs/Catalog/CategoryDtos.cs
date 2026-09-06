namespace AlHazmawi.Application.DTOs.Catalog;

public record CategoryDto(
    Guid Id,
    string NameArabic,
    string NameEnglish,
    string? IconUrl,
    string? IconEmoji,
    int SortOrder,
    int BusinessCount
);

public record CreateCategoryRequest(
    string NameArabic,
    string NameEnglish,
    string? IconUrl = null,
    string? IconEmoji = null,
    int SortOrder = 0
);

public record UpdateCategoryRequest(
    string NameArabic,
    string NameEnglish,
    string? IconUrl = null,
    string? IconEmoji = null,
    int SortOrder = 0,
    bool IsActive = true
);
