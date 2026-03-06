using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs;
using Backend.API.Security;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AdminOnly]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var users = await _db.Users.CountAsync();
            var sellers = await _db.SellerProfiles.CountAsync();
            var pendingSellers = await _db.SellerProfiles.CountAsync(sp => sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Pending);
            var orders = await _db.Orders.CountAsync();
            var paidOrders = await _db.Orders.CountAsync(o => o.PaymentStatus == Backend.Domain.PaymentStatus.Paid);
            var revenue = await _db.OrderItems.SumAsync(oi => (decimal?)oi.CommissionAmount) ?? 0;

            return Ok(new { users, sellers, pendingSellers, orders, paidOrders, revenue });
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> Analytics()
        {
            var totalRevenue = await _db.Orders
                .Where(o => o.PaymentStatus == Backend.Domain.PaymentStatus.Paid)
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

            var platformCommission = await _db.OrderItems.SumAsync(oi => (decimal?)oi.CommissionAmount) ?? 0;

            var topSellers = await _db.OrderItems
                .GroupBy(oi => oi.SellerId)
                .Select(g => new
                {
                    sellerId = g.Key,
                    totalSales = g.Sum(x => x.TotalPrice),
                    totalEarnings = g.Sum(x => x.SellerEarning)
                })
                .OrderByDescending(x => x.totalSales)
                .Take(5)
                .ToListAsync();

            var topProducts = await _db.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new
                {
                    productId = g.Key.ProductId,
                    productName = g.Key.Name,
                    totalQty = g.Sum(x => x.Quantity),
                    totalSales = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.totalQty)
                .Take(10)
                .ToListAsync();

            var dailyOrders = await _db.Orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { date = g.Key, count = g.Count() })
                .OrderByDescending(x => x.date)
                .Take(30)
                .ToListAsync();

            return Ok(new
            {
                totalRevenue,
                platformCommission,
                topSellers,
                topProducts,
                dailyOrders
            });
        }

        [HttpGet("sellers/pending")]
        public async Task<IActionResult> PendingSellers()
        {
            var sellers = await _db.SellerProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Pending)
                .AsNoTracking()
                .Select(sp => new
                {
                    sp.Id,
                    sp.UserId,
                    sp.ShopName,
                    sp.Bio,
                    sp.PhoneNumber,
                    sp.District,
                    sp.Upazila,
                    sp.CreatedAt,
                    sellerName = sp.User.FullName,
                    sellerEmail = sp.User.Email
                })
                .ToListAsync();

            return Ok(sellers);
        }

        [HttpPatch("sellers/{sellerProfileId:guid}/approval")]
        public async Task<IActionResult> UpdateSellerApproval(Guid sellerProfileId, [FromBody] SellerApprovalRequest req)
        {
            var profile = await _db.SellerProfiles.Include(x => x.User).SingleOrDefaultAsync(x => x.Id == sellerProfileId);
            if (profile == null) return NotFound();

            profile.ApprovalStatus = req.ApprovalStatus;
            profile.UpdatedAt = DateTime.UtcNow;

            if (req.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved)
                profile.User.Role = Backend.Domain.UserRole.Seller;
            else if (req.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Rejected)
                profile.User.Role = Backend.Domain.UserRole.User;

            await _db.SaveChangesAsync();
            return Ok(profile);
        }
    }
}
