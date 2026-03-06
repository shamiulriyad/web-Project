using System;
using System.Threading.Tasks;
using Backend.Application.DTOs;
using Backend.Data;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DiscountsController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponRequest req)
        {
            var code = req.Code.Trim().ToUpperInvariant();
            if (await _db.Discounts.AnyAsync(d => d.Code == code)) return BadRequest("Coupon code already exists");

            var coupon = new Discount
            {
                Id = Guid.NewGuid(),
                Code = code,
                Type = req.Type,
                Value = req.Value,
                MinimumPurchaseAmount = req.MinimumPurchaseAmount,
                UsageLimit = req.UsageLimit,
                ExpiryDate = req.ExpiryDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Discounts.AddAsync(coupon);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { code = coupon.Code }, coupon);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> Get(string code)
        {
            var d = await _db.Discounts.SingleOrDefaultAsync(x => x.Code == code && x.IsActive && (x.ExpiryDate == null || x.ExpiryDate > DateTime.UtcNow));
            if (d == null) return NotFound();
            return Ok(d);
        }

        [HttpGet("{code}/validate")]
        public async Task<IActionResult> ValidateCoupon(string code)
        {
            var coupon = await _db.Discounts.SingleOrDefaultAsync(x => x.Code == code && x.IsActive && (x.ExpiryDate == null || x.ExpiryDate > DateTime.UtcNow));
            if (coupon == null) return NotFound("Invalid coupon");
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value) return BadRequest("Coupon usage limit reached");

            return Ok(new
            {
                coupon.Code,
                coupon.Type,
                coupon.Value,
                coupon.MinimumPurchaseAmount,
                coupon.UsageLimit,
                coupon.UsedCount,
                coupon.ExpiryDate
            });
        }
    }
}
