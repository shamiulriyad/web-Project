using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.API.Security;
using Backend.Application.DTOs;
using Backend.Data;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/sellers")]
    [Route("api/seller")]
    public class SellersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SellersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] SellerApplicationRequest req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == req.UserId);
            if (user == null) return NotFound("User not found");

            var existing = await _db.SellerProfiles.SingleOrDefaultAsync(x => x.UserId == req.UserId);
            if (existing != null)
            {
                existing.ShopName = req.ShopName;
                existing.Bio = req.Bio;
                existing.PhoneNumber = req.PhoneNumber;
                existing.District = req.District;
                existing.Upazila = req.Upazila;
                existing.ApprovalStatus = Backend.Domain.SellerApprovalStatus.Pending;
                existing.UpdatedAt = DateTime.UtcNow;
                user.Role = Backend.Domain.UserRole.User;
                await _db.SaveChangesAsync();
                return Ok(existing);
            }

            var profile = new SellerProfile
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                ShopName = req.ShopName,
                Bio = req.Bio,
                PhoneNumber = req.PhoneNumber,
                District = req.District,
                Upazila = req.Upazila,
                ApprovalStatus = Backend.Domain.SellerApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            user.Role = Backend.Domain.UserRole.User;

            await _db.SellerProfiles.AddAsync(profile);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByUserId), new { userId = req.UserId }, profile);
        }

        [HttpGet("by-user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var seller = await _db.SellerProfiles
                .Include(sp => sp.User)
                .AsNoTracking()
                .SingleOrDefaultAsync(sp => sp.UserId == userId);

            if (seller == null) return NotFound();
            return Ok(seller);
        }

        [HttpGet("{userId:guid}/dashboard")]
        [SellerOnly]
        public async Task<IActionResult> Dashboard(Guid userId)
        {
            var sellerExists = await _db.SellerProfiles.AnyAsync(sp => sp.UserId == userId && sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved);
            if (!sellerExists) return BadRequest("Seller is not approved");

            var orderItems = _db.OrderItems.Where(oi => oi.SellerId == userId);

            var totalProducts = await _db.Products.CountAsync(p => p.SellerId == userId);
            var totalOrders = await orderItems.Select(oi => oi.OrderId).Distinct().CountAsync();
            var totalSales = await orderItems.SumAsync(oi => (decimal?)oi.TotalPrice) ?? 0;
            var totalCommission = await orderItems.SumAsync(oi => (decimal?)oi.CommissionAmount) ?? 0;
            var netEarnings = await orderItems.SumAsync(oi => (decimal?)oi.SellerEarning) ?? 0;

            return Ok(new
            {
                totalProducts,
                totalOrders,
                totalSales,
                totalCommission,
                netEarnings
            });
        }

        [HttpGet("{userId:guid}/sales/monthly")]
        [SellerOnly]
        public async Task<IActionResult> MonthlySales(Guid userId)
        {
            var sellerExists = await _db.SellerProfiles.AnyAsync(sp => sp.UserId == userId && sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved);
            if (!sellerExists) return BadRequest("Seller is not approved");

            var since = DateTime.UtcNow.AddMonths(-12);
            var monthly = await _db.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.SellerId == userId && oi.Order.CreatedAt >= since)
                .GroupBy(oi => new { oi.Order.CreatedAt.Year, oi.Order.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    totalSales = g.Sum(x => x.TotalPrice),
                    netRevenue = g.Sum(x => x.SellerEarning)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(monthly);
        }

        [HttpGet("orders")]
        [SellerOnly]
        public async Task<IActionResult> GetOrders([FromQuery] Guid sellerId)
        {
            var sellerExists = await _db.SellerProfiles.AnyAsync(sp => sp.UserId == sellerId && sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved);
            if (!sellerExists) return BadRequest("Seller is not approved");

            var orders = await _db.OrderItems
                .Where(oi => oi.SellerId == sellerId)
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .AsNoTracking()
                .Select(oi => new
                {
                    oi.OrderId,
                    oi.ProductId,
                    productName = oi.Product.Name,
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.TotalPrice,
                    oi.CommissionAmount,
                    oi.SellerEarning,
                    orderStatus = oi.Order.OrderStatus,
                    paymentStatus = oi.Order.PaymentStatus,
                    oi.Order.CreatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpPut("orders/{orderId:guid}/status")]
        [SellerOnly]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromQuery] Guid sellerId, [FromBody] UpdateOrderStatusRequest req)
        {
            var sellerExists = await _db.SellerProfiles.AnyAsync(sp => sp.UserId == sellerId && sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved);
            if (!sellerExists) return BadRequest("Seller is not approved");

            var isSellerOrder = await _db.OrderItems.AnyAsync(oi => oi.OrderId == orderId && oi.SellerId == sellerId);
            if (!isSellerOrder) return Forbid();

            var order = await _db.Orders.SingleOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            if (!IsValidStatusTransition(order.OrderStatus, req.OrderStatus))
                return BadRequest($"Invalid transition from {order.OrderStatus} to {req.OrderStatus}");

            if (req.OrderStatus == Backend.Domain.OrderStatus.Confirmed && order.PaymentStatus != Backend.Domain.PaymentStatus.Paid)
                return BadRequest("Order cannot be confirmed before payment verification");

            var previousStatus = order.OrderStatus;

            if (req.OrderStatus == Backend.Domain.OrderStatus.Delivered)
            {
                var orderItems = await _db.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
                var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();
                var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

                foreach (var item in orderItems)
                {
                    if (!products.TryGetValue(item.ProductId, out var product)) continue;

                    product.ReservedStock = Math.Max(0, product.ReservedStock - item.Quantity);
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
                    product.UpdatedAt = DateTime.UtcNow;

                    if (product.StockQuantity - product.ReservedStock <= product.LowStockAlertThreshold)
                    {
                        _db.Notifications.Add(new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = product.SellerId,
                            Title = "Low stock alert",
                            Message = $"{product.Name} stock is low ({product.StockQuantity - product.ReservedStock} available)",
                            Channel = Backend.Domain.NotificationChannel.InApp,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            if (req.OrderStatus == Backend.Domain.OrderStatus.Cancelled && previousStatus is Backend.Domain.OrderStatus.Pending or Backend.Domain.OrderStatus.Confirmed or Backend.Domain.OrderStatus.Processing)
            {
                var orderItems = await _db.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
                var productIds = orderItems.Select(oi => oi.ProductId).Distinct().ToList();
                var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

                foreach (var item in orderItems)
                {
                    if (!products.TryGetValue(item.ProductId, out var product)) continue;
                    product.ReservedStock = Math.Max(0, product.ReservedStock - item.Quantity);
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            order.OrderStatus = req.OrderStatus;
            order.UpdatedAt = DateTime.UtcNow;

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = order.UserId,
                Title = "Order update",
                Message = $"Your order {order.Id} status is now {req.OrderStatus}.",
                Channel = Backend.Domain.NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return Ok(order);
        }

        [HttpPut("products/{productId:guid}/stock")]
        [SellerOnly]
        public async Task<IActionResult> UpdateStock(Guid productId, [FromQuery] Guid sellerId, [FromBody] UpdateStockRequest req)
        {
            var product = await _db.Products.SingleOrDefaultAsync(p => p.Id == productId && p.SellerId == sellerId);
            if (product == null) return NotFound();

            product.StockQuantity = req.StockQuantity;
            if (req.LowStockAlertThreshold.HasValue)
                product.LowStockAlertThreshold = req.LowStockAlertThreshold.Value;
            product.UpdatedAt = DateTime.UtcNow;

            if (product.StockQuantity - product.ReservedStock <= product.LowStockAlertThreshold)
            {
                _db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = sellerId,
                    Title = "Low stock alert",
                    Message = $"{product.Name} is below threshold.",
                    Channel = Backend.Domain.NotificationChannel.InApp,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return Ok(product);
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