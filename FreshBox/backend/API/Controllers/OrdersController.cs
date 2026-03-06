using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.API.Security;
using Backend.Application.DTOs;
using Backend.Data;
using Backend.Domain.Entities;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OrdersController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        [CustomerOnly]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId && u.IsActive);
            if (!userExists) return BadRequest("Invalid user");

            var addressExists = await _db.UserAddresses.AnyAsync(a => a.Id == dto.AddressId && a.UserId == dto.UserId);
            if (!addressExists) return BadRequest("Invalid address");

            var lowStockNotifications = new List<Notification>();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                AddressId = dto.AddressId,
                PaymentMethod = dto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                OrderStatus = Backend.Domain.OrderStatus.Pending,
                PaymentStatus = Backend.Domain.PaymentStatus.Pending
            };

            foreach (var it in dto.Items)
            {
                var product = await _db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == it.ProductId && p.IsActive);
                if (product == null) return BadRequest($"Invalid product: {it.ProductId}");

                var availableStock = product.StockQuantity - product.ReservedStock;
                if (availableStock < it.Quantity)
                    return BadRequest($"Insufficient stock for product: {product.Name}");

                var commissionRate = await ResolveCommissionRate(product.SellerId, product.CategoryId);

                var lineTotal = it.UnitPrice * it.Quantity;
                var commission = Math.Round(lineTotal * (commissionRate / 100m), 2);

                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = it.ProductId,
                    SellerId = product.SellerId,
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice,
                    TotalPrice = lineTotal,
                    CommissionAmount = commission,
                    SellerEarning = lineTotal - commission
                });

                product.ReservedStock += it.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                _db.Products.Update(product);

                var remainingAfterReserve = product.StockQuantity - product.ReservedStock;
                if (remainingAfterReserve <= product.LowStockAlertThreshold)
                {
                    lowStockNotifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = product.SellerId,
                        Title = "Low stock alert",
                        Message = $"{product.Name} stock is low ({remainingAfterReserve} left after reservation)",
                        Channel = Backend.Domain.NotificationChannel.InApp,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

            decimal discountAmount = 0;
            if (!string.IsNullOrWhiteSpace(dto.DiscountCode))
            {
                var discount = await _db.Discounts
                    .SingleOrDefaultAsync(d => d.Code == dto.DiscountCode && d.IsActive && (d.ExpiryDate == null || d.ExpiryDate > DateTime.UtcNow));

                if (discount == null) return BadRequest("Invalid coupon");
                if (discount.MinimumPurchaseAmount.HasValue && order.TotalAmount < discount.MinimumPurchaseAmount.Value)
                    return BadRequest("Minimum purchase amount not met for this coupon");
                if (discount.UsageLimit.HasValue && discount.UsedCount >= discount.UsageLimit.Value)
                    return BadRequest("Coupon usage limit reached");

                discountAmount = string.Equals(discount.Type, "Percentage", StringComparison.OrdinalIgnoreCase)
                    ? Math.Round(order.TotalAmount * (discount.Value / 100m), 2)
                    : discount.Value;

                if (discountAmount > order.TotalAmount)
                    discountAmount = order.TotalAmount;

                discount.UsedCount += 1;

                _db.DiscountUsages.Add(new DiscountUsage
                {
                    Id = Guid.NewGuid(),
                    DiscountId = discount.Id,
                    UserId = dto.UserId,
                    UsedAt = DateTime.UtcNow
                });
            }

            order.DiscountAmount = discountAmount;
            order.FinalAmount = order.TotalAmount - discountAmount;

            await _db.Orders.AddAsync(order);

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Title = "Order placed",
                Message = $"Your order {order.Id} has been placed successfully.",
                Channel = Backend.Domain.NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow
            });

            if (lowStockNotifications.Count > 0)
                await _db.Notifications.AddRangeAsync(lowStockNotifications);

            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        [HttpPost("checkout-from-cart")]
        [CustomerOnly]
        public async Task<IActionResult> CheckoutFromCart([FromBody] CheckoutFromCartRequest req)
        {
            var cart = await _db.Carts.SingleOrDefaultAsync(c => c.UserId == req.UserId);
            if (cart == null) return BadRequest("Cart is empty");

            var cartItems = await _db.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .Include(ci => ci.Product)
                .ToListAsync();

            if (!cartItems.Any()) return BadRequest("Cart is empty");

            var dto = new CreateOrderDto(
                req.UserId,
                req.AddressId,
                req.PaymentMethod,
                cartItems.Select(ci => new OrderItemDto(ci.ProductId, ci.Quantity, ci.Product.DiscountPrice ?? ci.Product.Price)),
                req.DiscountCode
            );

            var result = await Create(dto);

            _db.CartItems.RemoveRange(cartItems);
            await _db.SaveChangesAsync();

            return result;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .SingleOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpGet("user/orders")]
        [CustomerOnly]
        public async Task<IActionResult> UserOrders([FromQuery] Guid userId)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPut("{id:guid}/status")]
        [SellerOnly]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatusUpdateRequest req)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            if (!IsValidStatusTransition(order.OrderStatus, req.Status))
                return BadRequest($"Invalid transition from {order.OrderStatus} to {req.Status}");

            order.OrderStatus = req.Status;
            order.UpdatedAt = DateTime.UtcNow;

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = order.UserId,
                Title = "Order update",
                Message = $"Your order {order.Id} status is now {req.Status}.",
                Channel = Backend.Domain.NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return Ok(order);
        }

        private async Task<decimal> ResolveCommissionRate(Guid sellerId, Guid categoryId)
        {
            var sellerRule = await _db.CommissionRules
                .AsNoTracking()
                .Where(r => r.IsActive && r.SellerId == sellerId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .FirstOrDefaultAsync();
            if (sellerRule != null) return sellerRule.Percentage;

            var categoryRule = await _db.CommissionRules
                .AsNoTracking()
                .Where(r => r.IsActive && r.CategoryId == categoryId && r.SellerId == null)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .FirstOrDefaultAsync();
            if (categoryRule != null) return categoryRule.Percentage;

            var globalRule = await _db.CommissionRules
                .AsNoTracking()
                .Where(r => r.IsActive && r.SellerId == null && r.CategoryId == null)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .FirstOrDefaultAsync();
            if (globalRule != null) return globalRule.Percentage;

            return 10m;
        }

        private static bool IsValidStatusTransition(Backend.Domain.OrderStatus current, Backend.Domain.OrderStatus next)
        {
            if (current == next) return true;

            return current switch
            {
                Backend.Domain.OrderStatus.Pending => next is Backend.Domain.OrderStatus.Cancelled or Backend.Domain.OrderStatus.Confirmed,
                Backend.Domain.OrderStatus.Confirmed => next is Backend.Domain.OrderStatus.Processing or Backend.Domain.OrderStatus.Cancelled,
                Backend.Domain.OrderStatus.Processing => next is Backend.Domain.OrderStatus.Shipped or Backend.Domain.OrderStatus.Cancelled,
                Backend.Domain.OrderStatus.Shipped => next is Backend.Domain.OrderStatus.Delivered,
                Backend.Domain.OrderStatus.Delivered => false,
                Backend.Domain.OrderStatus.Cancelled => false,
                _ => false
            };
        }
    }
}
