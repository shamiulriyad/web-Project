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
    [Route("api/seller/payouts")]
    [Route("api/sellers/payouts")]
    [Route("api/admin/payouts")]
    [Route("seller")]
    [Route("admin/payout")]
    public class PayoutsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public PayoutsController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        [AdminOnly]
        public async Task<IActionResult> Get([FromQuery] Guid? sellerId, [FromQuery] Backend.Domain.PayoutStatus? status)
        {
            var query = _db.Payouts.AsNoTracking().AsQueryable();
            if (sellerId.HasValue) query = query.Where(p => p.SellerId == sellerId.Value);
            if (status.HasValue) query = query.Where(p => p.Status == status.Value);

            var payouts = await query.OrderByDescending(p => p.RequestedAt).ToListAsync();
            return Ok(payouts);
        }

        [HttpPost("request")]
        [HttpPost("payout-request")]
        [SellerOnly]
        public async Task<IActionResult> RequestPayout([FromBody] PayoutRequestDto req)
        {
            var sellerExists = await _db.SellerProfiles.AnyAsync(sp => sp.UserId == req.SellerId && sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved);
            if (!sellerExists) return BadRequest("Seller is not approved");

            var eligibleItems = await _db.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.SellerId == req.SellerId
                    && oi.PayoutId == null
                    && oi.Order.OrderStatus == Backend.Domain.OrderStatus.Delivered)
                .ToListAsync();

            if (!eligibleItems.Any()) return BadRequest("No delivered earnings available for payout");

            var amount = eligibleItems.Sum(x => x.SellerEarning);
            var payout = new Payout
            {
                Id = Guid.NewGuid(),
                SellerId = req.SellerId,
                Amount = amount,
                Status = Backend.Domain.PayoutStatus.Requested,
                RequestedAt = DateTime.UtcNow
            };

            await _db.Payouts.AddAsync(payout);
            foreach (var item in eligibleItems) item.PayoutId = payout.Id;

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = req.SellerId,
                Title = "Payout requested",
                Message = $"Payout request of {amount} has been submitted.",
                Channel = Backend.Domain.NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { sellerId = req.SellerId }, payout);
        }

        [HttpPatch("{payoutId:guid}/decision")]
        [HttpPut("{payoutId:guid}/approve")]
        [AdminOnly]
        public async Task<IActionResult> Decide(Guid payoutId, [FromBody] PayoutDecisionDto req)
        {
            var payout = await _db.Payouts.SingleOrDefaultAsync(p => p.Id == payoutId);
            if (payout == null) return NotFound();

            payout.Status = req.Status;
            if (req.Status == Backend.Domain.PayoutStatus.Paid || req.Status == Backend.Domain.PayoutStatus.Approved)
                payout.PaidAt = DateTime.UtcNow;

            if (req.Status == Backend.Domain.PayoutStatus.Rejected)
            {
                var linkedItems = await _db.OrderItems.Where(oi => oi.PayoutId == payout.Id).ToListAsync();
                foreach (var item in linkedItems) item.PayoutId = null;
            }

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = payout.SellerId,
                Title = "Payout updated",
                Message = $"Your payout {payout.Id} status is now {payout.Status}.",
                Channel = Backend.Domain.NotificationChannel.InApp,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return Ok(payout);
        }
    }
}