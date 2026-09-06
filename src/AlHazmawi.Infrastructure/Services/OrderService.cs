using System.Text.Json;
using AlHazmawi.Application.Common.Interfaces;
using AlHazmawi.Application.DTOs.Orders;
using AlHazmawi.Domain.Entities;
using AlHazmawi.Domain.Enums;
using AlHazmawi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlHazmawi.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AlHazmawiDbContext _context;
    private readonly IOrderNotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        AlHazmawiDbContext context,
        IOrderNotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<OrderDetailDto> CreateOrderAsync(CreateOrderRequest request, string userId)
    {
        if (request.Items == null || !request.Items.Any())
            throw new InvalidOperationException("Order must contain at least one item / يجب أن يحتوي الطلب على صنف واحد على الأقل");

        var customer = await _context.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
            throw new InvalidOperationException("Customer profile not found / لم يتم العثور على حساب الزبون");

        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == request.BusinessId && b.IsActive && b.Status == BusinessStatus.Approved);

        if (business == null)
            throw new InvalidOperationException("Store is currently unavailable / المتجر غير متاح حالياً لاستقبال الطلبات");

        // Resolve delivery address
        string addressLine = "";
        double lat = 32.0;
        double lon = 35.0;
        string phone = customer.Phone;

        if (request.AddressId.HasValue)
        {
            var addr = customer.Addresses.FirstOrDefault(a => a.Id == request.AddressId.Value);
            if (addr != null)
            {
                addressLine = $"{addr.Label}: {addr.AddressLine}";
                lat = addr.Latitude;
                lon = addr.Longitude;
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.CustomAddressLine))
        {
            addressLine = request.CustomAddressLine;
            lat = request.DeliveryLatitude ?? 32.0;
            lon = request.DeliveryLongitude ?? 35.0;
            phone = request.DeliveryPhone ?? customer.Phone;
        }
        else
        {
            var defaultAddr = customer.Addresses.FirstOrDefault(a => a.IsDefault);
            if (defaultAddr != null)
            {
                addressLine = $"{defaultAddr.Label}: {defaultAddr.AddressLine}";
                lat = defaultAddr.Latitude;
                lon = defaultAddr.Longitude;
            }
            else
            {
                throw new InvalidOperationException("Please specify a delivery address / يرجى تحديد عنوان التوصيل");
            }
        }

        // Fetch products and calculate server-side subtotal
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Include(p => p.Options)
            .Where(p => productIds.Contains(p.Id) && p.BusinessId == request.BusinessId)
            .ToDictionaryAsync(p => p.Id);

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var itemReq in request.Items)
        {
            if (!products.TryGetValue(itemReq.ProductId, out var product))
                throw new InvalidOperationException($"Product {itemReq.ProductId} does not belong to this store or was not found");

            if (!product.IsAvailable)
                throw new InvalidOperationException($"Product '{product.NameArabic}' is currently unavailable / هذا الصنف غير متوفر حالياً");

            decimal itemUnitPrice = product.Price;
            var selectedOptionsList = new List<object>();

            if (itemReq.SelectedOptionIds != null && itemReq.SelectedOptionIds.Any())
            {
                var options = product.Options
                    .Where(o => itemReq.SelectedOptionIds.Contains(o.Id) && o.IsActive)
                    .ToList();

                foreach (var opt in options)
                {
                    itemUnitPrice += opt.AdditionalPrice;
                    selectedOptionsList.Add(new
                    {
                        opt.Id,
                        opt.NameArabic,
                        opt.NameEnglish,
                        opt.AdditionalPrice
                    });
                }
            }

            var lineTotal = itemUnitPrice * itemReq.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductNameArabic = product.NameArabic,
                ProductNameEnglish = product.NameEnglish,
                UnitPrice = itemUnitPrice,
                Quantity = itemReq.Quantity,
                LineTotal = lineTotal,
                Notes = itemReq.Notes,
                SelectedOptions = selectedOptionsList.Any() ? JsonSerializer.Serialize(selectedOptionsList) : null
            });
        }

        if (business.MinimumOrderAmount.HasValue && subtotal < business.MinimumOrderAmount.Value)
        {
            throw new InvalidOperationException($"Minimum order amount for this store is {business.MinimumOrderAmount:N2} / الحد الأدنى للطلب هو {business.MinimumOrderAmount:N2}");
        }

        // Validate coupon if provided
        decimal discount = 0;
        Guid? couponId = null;
        string? couponCode = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var code = request.CouponCode.Trim().ToUpper();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c =>
                c.Code.ToUpper() == code &&
                c.IsActive &&
                c.StartDate <= DateTime.UtcNow &&
                c.ExpiryDate >= DateTime.UtcNow);

            if (coupon != null)
            {
                if (coupon.MinimumOrderAmount == null || subtotal >= coupon.MinimumOrderAmount)
                {
                    if (coupon.Type == CouponType.Percentage)
                    {
                        discount = (subtotal * coupon.Value) / 100m;
                        if (coupon.MaximumDiscount.HasValue && discount > coupon.MaximumDiscount.Value)
                            discount = coupon.MaximumDiscount.Value;
                    }
                    else
                    {
                        discount = coupon.Value;
                    }

                    if (discount > subtotal) discount = subtotal;

                    couponId = coupon.Id;
                    couponCode = coupon.Code;
                    coupon.UsageCount++;
                }
            }
        }

        var deliveryFee = business.DeliveryFee;
        var total = subtotal + deliveryFee - discount;

        // Generate human-readable order number: HZ-YYYYMMDD-XXXX
        var randomSuffix = Random.Shared.Next(1000, 9999);
        var orderNumber = $"HZ-{DateTime.UtcNow:yyyyMMdd}-{randomSuffix}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customer.Id,
            BusinessId = business.Id,
            DeliveryAddressLine = addressLine,
            DeliveryLatitude = lat,
            DeliveryLongitude = lon,
            DeliveryPhone = phone,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            Total = total,
            CouponId = couponId,
            CouponCode = couponCode,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            Status = OrderStatus.Pending,
            CustomerNote = request.CustomerNote,
            Items = orderItems
        };

        // Status history
        order.StatusHistory.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Pending,
            ActorId = userId,
            ActorRole = "Customer",
            Note = "تم إرسال الطلب وبانتظار موافقة المتجر",
            ChangedAt = DateTime.UtcNow
        });

        // Payment record
        order.Payment = new Payment
        {
            Amount = total,
            Method = request.PaymentMethod,
            Status = PaymentStatus.Pending
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order created: {OrderNumber} with Total: {Total}", order.OrderNumber, order.Total);

        // Send real-time notification to the business
        await _notificationService.NotifyNewOrderToBusinessAsync(business.Id, order.Id, order.OrderNumber);

        return await GetOrderDetailsDtoAsync(order.Id);
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, string userId, string role)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Business)
            .Include(o => o.Driver)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        // Verify authorization
        if (role == "Customer" && order.Customer.UserId != userId) return null;
        if (role == "Business" && order.Business.UserId != userId) return null;
        if (role == "Driver" && order.Driver?.UserId != userId && order.Status != OrderStatus.Accepted && order.Status != OrderStatus.Preparing) return null;

        return await GetOrderDetailsDtoAsync(orderId);
    }

    public async Task<List<OrderSummaryDto>> GetCustomerOrdersAsync(string userId)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null) return new List<OrderSummaryDto>();

        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customer.Id)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.BusinessId,
                o.Business.NameArabic,
                o.Business.NameEnglish,
                o.Business.LogoUrl,
                o.Status,
                o.Total,
                o.CreatedAt,
                o.Items.Count
            ))
            .ToListAsync();
    }

    public async Task<List<OrderSummaryDto>> GetBusinessOrdersAsync(Guid businessId, OrderStatus? status = null)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.BusinessId == businessId);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.BusinessId,
                o.Business.NameArabic,
                o.Business.NameEnglish,
                o.Business.LogoUrl,
                o.Status,
                o.Total,
                o.CreatedAt,
                o.Items.Count
            ))
            .ToListAsync();
    }

    public async Task<List<OrderSummaryDto>> GetDriverOrdersAsync(string userId, bool activeOnly = true)
    {
        var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
        if (driver == null) return new List<OrderSummaryDto>();

        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.DriverId == driver.Id);

        if (activeOnly)
        {
            query = query.Where(o =>
                o.Status == OrderStatus.DriverAssigned ||
                o.Status == OrderStatus.PickedUp ||
                o.Status == OrderStatus.OnTheWay);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.BusinessId,
                o.Business.NameArabic,
                o.Business.NameEnglish,
                o.Business.LogoUrl,
                o.Status,
                o.Total,
                o.CreatedAt,
                o.Items.Count
            ))
            .ToListAsync();
    }

    public async Task<List<OrderSummaryDto>> GetAvailableOrdersForDriversAsync(double? currentLat, double? currentLon)
    {
        // Available orders: Accepted or Preparing, and no driver assigned yet
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.DriverId == null && (o.Status == OrderStatus.Accepted || o.Status == OrderStatus.Preparing))
            .OrderBy(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.BusinessId,
                o.Business.NameArabic,
                o.Business.NameEnglish,
                o.Business.LogoUrl,
                o.Status,
                o.Total,
                o.CreatedAt,
                o.Items.Count
            ))
            .ToListAsync();
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, string userId, string role)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Business)
            .Include(o => o.Driver)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return false;

        // Validate transitions and permissions
        if (role == "Business")
        {
            if (order.Business.UserId != userId) return false;

            if (request.NewStatus == OrderStatus.Accepted)
            {
                order.Status = OrderStatus.Accepted;
                order.AcceptedAt = DateTime.UtcNow;
            }
            else if (request.NewStatus == OrderStatus.Preparing)
            {
                order.Status = OrderStatus.Preparing;
            }
            else if (request.NewStatus == OrderStatus.Rejected)
            {
                order.Status = OrderStatus.Rejected;
                order.RejectionReason = request.RejectionReason ?? "المتجر غير قادر على تلبية الطلب حالياً";
            }
            else
            {
                return false;
            }
        }
        else if (role == "Driver")
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null) return false;

            // Self-assign if picking up available order
            if (order.DriverId == null && request.NewStatus == OrderStatus.DriverAssigned)
            {
                order.DriverId = driver.Id;
                order.Status = OrderStatus.DriverAssigned;
            }
            else if (order.DriverId == driver.Id)
            {
                if (request.NewStatus == OrderStatus.PickedUp)
                {
                    order.Status = OrderStatus.PickedUp;
                }
                else if (request.NewStatus == OrderStatus.OnTheWay)
                {
                    order.Status = OrderStatus.OnTheWay;
                }
                else if (request.NewStatus == OrderStatus.Delivered)
                {
                    order.Status = OrderStatus.Delivered;
                    order.DeliveredAt = DateTime.UtcNow;
                    if (order.Payment != null && order.PaymentMethod == PaymentMethod.CashOnDelivery)
                    {
                        order.Payment.Status = PaymentStatus.Completed;
                        order.Payment.PaidAt = DateTime.UtcNow;
                        order.PaymentStatus = PaymentStatus.Completed;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else if (role == "Customer")
        {
            if (order.Customer.UserId != userId) return false;
            if (order.Status == OrderStatus.Pending && request.NewStatus == OrderStatus.Cancelled)
            {
                order.Status = OrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
            }
            else
            {
                return false;
            }
        }
        else if (role == "Admin")
        {
            order.Status = request.NewStatus;
        }

        // Record history
        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = order.Status,
            ActorId = userId,
            ActorRole = role,
            Note = request.Note,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Broadcast real-time update via SignalR
        await _notificationService.NotifyOrderStatusChangedAsync(
            order.Id,
            order.CustomerId,
            order.Status,
            request.Note);

        return true;
    }

    public async Task<bool> AssignDriverAsync(Guid orderId, Guid driverId, string userId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return false;

        var driver = await _context.Drivers.FindAsync(driverId);
        if (driver == null) return false;

        order.DriverId = driverId;
        order.Status = OrderStatus.DriverAssigned;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.DriverAssigned,
            ActorId = userId,
            ActorRole = "System/Business",
            Note = $"تم تعيين المندوب: {driver.FullName}",
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Notify driver and customer
        await _notificationService.NotifyDriverAssignedAsync(driverId, order.Id);
        await _notificationService.NotifyOrderStatusChangedAsync(order.Id, order.CustomerId, order.Status, $"المندوب {driver.FullName} في طريقه لاستلام طلبك");

        return true;
    }

    private async Task<OrderDetailDto> GetOrderDetailsDtoAsync(Guid orderId)
    {
        var o = await _context.Orders
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Business)
            .Include(x => x.Driver)
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .FirstAsync(x => x.Id == orderId);

        return new OrderDetailDto(
            o.Id,
            o.OrderNumber,
            o.CustomerId,
            o.Customer.FullName,
            o.Customer.Phone,
            o.BusinessId,
            o.Business.NameArabic,
            o.Business.NameEnglish,
            o.Business.Phone,
            o.DriverId,
            o.Driver?.FullName,
            o.Driver?.Phone,
            o.DeliveryAddressLine,
            o.DeliveryLatitude,
            o.DeliveryLongitude,
            o.DeliveryPhone,
            o.Subtotal,
            o.DeliveryFee,
            o.Discount,
            o.Total,
            o.PaymentMethod,
            o.PaymentStatus,
            o.Status,
            o.CustomerNote,
            o.RejectionReason,
            o.CreatedAt,
            o.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductNameArabic,
                i.ProductNameEnglish,
                i.UnitPrice,
                i.Quantity,
                i.LineTotal,
                i.Notes,
                i.SelectedOptions
            )).ToList(),
            o.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new OrderStatusHistoryDto(
                    h.Status,
                    h.Note,
                    h.ChangedAt,
                    h.ActorRole
                )).ToList()
        );
    }
}
