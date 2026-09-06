using AlHazmawi.Application.DTOs.Business;

namespace AlHazmawi.Application.Common.Interfaces;

public interface IBusinessService
{
    Task<List<BusinessDto>> GetBusinessesAsync(BusinessQueryParameters parameters);
    Task<BusinessDetailDto?> GetBusinessDetailsAsync(Guid id);
    Task<BusinessDto?> GetBusinessByUserIdAsync(string userId);
    Task<bool> UpdateBusinessProfileAsync(Guid businessId, UpdateBusinessProfileRequest request, string userId);
    Task<bool> ToggleBusinessOpenStatusAsync(Guid businessId, string userId);
    Task<List<BusinessDto>> GetPendingBusinessesAsync();
    Task<bool> ApproveBusinessAsync(Guid businessId);
    Task<bool> RejectBusinessAsync(Guid businessId, string? reason);
}
