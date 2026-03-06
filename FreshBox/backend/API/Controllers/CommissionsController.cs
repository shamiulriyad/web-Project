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
    [Route("api/admin/commissions")]
    [AdminOnly]
    public class CommissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CommissionsController(ApplicationDbContext db) => _db = db;

        [HttpGet("rules")]
        public async Task<IActionResult> Rules()
        {
            var rules = await _db.CommissionRules
                .AsNoTracking()
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();

            return Ok(rules);
        }

        [HttpPost("rules")]
        public async Task<IActionResult> Upsert([FromBody] CommissionRuleRequest req)
        {
            if (req.Percentage < 0 || req.Percentage > 100) return BadRequest("Commission must be between 0 and 100");

            var existing = await _db.CommissionRules.SingleOrDefaultAsync(r => r.SellerId == req.SellerId && r.CategoryId == req.CategoryId);
            if (existing != null)
            {
                existing.Percentage = req.Percentage;
                existing.IsActive = req.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(existing);
            }

            var rule = new CommissionRule
            {
                Id = Guid.NewGuid(),
                SellerId = req.SellerId,
                CategoryId = req.CategoryId,
                Percentage = req.Percentage,
                IsActive = req.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _db.CommissionRules.AddAsync(rule);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Rules), new { id = rule.Id }, rule);
        }

        [HttpGet("effective")]
        public async Task<IActionResult> Effective([FromQuery] Guid sellerId, [FromQuery] Guid categoryId)
        {
            var sellerRule = await _db.CommissionRules.AsNoTracking().Where(r => r.IsActive && r.SellerId == sellerId).OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).FirstOrDefaultAsync();
            if (sellerRule != null) return Ok(new { source = "Seller", percentage = sellerRule.Percentage });

            var categoryRule = await _db.CommissionRules.AsNoTracking().Where(r => r.IsActive && r.SellerId == null && r.CategoryId == categoryId).OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).FirstOrDefaultAsync();
            if (categoryRule != null) return Ok(new { source = "Category", percentage = categoryRule.Percentage });

            var globalRule = await _db.CommissionRules.AsNoTracking().Where(r => r.IsActive && r.SellerId == null && r.CategoryId == null).OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt).FirstOrDefaultAsync();
            if (globalRule != null) return Ok(new { source = "Global", percentage = globalRule.Percentage });

            return Ok(new { source = "Default", percentage = 10m });
        }
    }
}