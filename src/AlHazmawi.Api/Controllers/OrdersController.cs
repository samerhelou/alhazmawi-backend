using System.Security.Claims;
using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Orders;
using AlHazmawi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlHazmawi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new delivery order (Customer only). Server calculates all prices and discounts.
    /// </summary>
    [Authorize(Roles = "Customer")]
    [HttpPost]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var order = await _orderService.CreateOrderAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets detailed order information by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Customer";
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(id, userId, role);
        if (order == null) return NotFound("Order not found or unauthorized / الطلب غير موجود");

        return Ok(order);
    }

    /// <summary>
    /// Gets order history for the logged-in customer.
    /// </summary>
    [Authorize(Roles = "Customer")]
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var orders = await _orderService.GetCustomerOrdersAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// Gets orders for a specific store (Merchant/Admin).
    /// </summary>
    [Authorize(Roles = "Business,Admin")]
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBusinessOrders(Guid businessId, [FromQuery] OrderStatus? status = null)
    {
        var orders = await _orderService.GetBusinessOrdersAsync(businessId, status);
        return Ok(orders);
    }

    /// <summary>
    /// Gets active delivery assignments for the logged-in driver.
    /// </summary>
    [Authorize(Roles = "Driver")]
    [HttpGet("driver/my-deliveries")]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDeliveries([FromQuery] bool activeOnly = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var orders = await _orderService.GetDriverOrdersAsync(userId, activeOnly);
        return Ok(orders);
    }

    /// <summary>
    /// Gets available orders waiting for driver pickup (Driver/Admin).
    /// </summary>
    [Authorize(Roles = "Driver,Admin")]
    [HttpGet("driver/available")]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableOrders([FromQuery] double? lat = null, [FromQuery] double? lon = null)
    {
        var orders = await _orderService.GetAvailableOrdersForDriversAsync(lat, lon);
        return Ok(orders);
    }

    /// <summary>
    /// Updates order lifecycle status (Accept, Prepare, Pickup, On the way, Delivered, Cancel, Reject).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _orderService.UpdateOrderStatusAsync(id, request, userId, role);
        if (!success)
            return BadRequest(new { Error = "Invalid status transition or not authorized / تغيير الحالة غير مسموح" });

        return Ok(new { Message = $"Order status updated to {request.NewStatus}" });
    }

    /// <summary>
    /// Assigns a delivery driver to an order (Merchant/Admin).
    /// </summary>
    [Authorize(Roles = "Business,Admin")]
    [HttpPatch("{id:guid}/assign-driver")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignDriver(Guid id, [FromBody] AssignDriverRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var success = await _orderService.AssignDriverAsync(id, request.DriverId, userId);
        if (!success)
            return BadRequest(new { Error = "Could not assign driver / تعذر تعيين المندوب" });

        return Ok(new { Message = "Driver assigned successfully" });
    }
}
