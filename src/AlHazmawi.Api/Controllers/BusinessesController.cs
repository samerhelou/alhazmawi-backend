using System.Security.Claims;
using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlHazmawi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessesController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    /// <summary>
    /// Gets all active businesses with optional filters (category, search, GPS distance).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BusinessDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusinesses([FromQuery] BusinessQueryParameters parameters)
    {
        var businesses = await _businessService.GetBusinessesAsync(parameters);
        return Ok(businesses);
    }

    /// <summary>
    /// Gets full store details including working hours and menu catalog.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BusinessDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBusinessDetails(Guid id)
    {
        var business = await _businessService.GetBusinessDetailsAsync(id);
        if (business == null) return NotFound("Business not found / المتجر غير موجود");
        return Ok(business);
    }

    /// <summary>
    /// Gets the business profile of the currently logged-in merchant.
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpGet("my-business")]
    [ProducesResponseType(typeof(BusinessDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyBusiness()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var business = await _businessService.GetBusinessByUserIdAsync(userId);
        if (business == null) return NotFound("No business associated with your account.");

        return Ok(business);
    }

    /// <summary>
    /// Updates business profile details (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateBusinessProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _businessService.UpdateBusinessProfileAsync(id, request, userId);
        if (!success) return BadRequest("Could not update business. Verify ownership.");

        return Ok(new { Message = "Business profile updated successfully" });
    }

    /// <summary>
    /// Toggles store active/busy/closed status (Merchant only).
    /// </summary>
    [Authorize(Roles = "Business")]
    [HttpPatch("{id:guid}/toggle-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var isNowActive = await _businessService.ToggleBusinessOpenStatusAsync(id, userId);
        return Ok(new { IsActive = isNowActive });
    }

    /// <summary>
    /// Gets all pending businesses (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var list = await _businessService.GetPendingBusinessesAsync();
        return Ok(list);
    }

    /// <summary>
    /// Approves a pending business (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _businessService.ApproveBusinessAsync(id);
        if (!ok) return NotFound("Business not found");
        return Ok(new { Message = "Business approved" });
    }

    /// <summary>
    /// Rejects a pending business (Admin only).
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectBusinessRequest? request)
    {
        var ok = await _businessService.RejectBusinessAsync(id, request?.Reason);
        if (!ok) return NotFound("Business not found");
        return Ok(new { Message = "Business rejected" });
    }

    public record RejectBusinessRequest(string? Reason);
}
