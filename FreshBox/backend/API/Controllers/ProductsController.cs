using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? q, [FromQuery] Guid? sellerId, [FromQuery] Guid? categoryId, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] decimal? minRating, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var query = _db.Products.AsQueryable().Where(p => p.IsActive);
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p => p.Name.Contains(q));
            if (sellerId.HasValue) query = query.Where(p => p.SellerId == sellerId.Value);
            if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
            if (minPrice.HasValue) query = query.Where(p => (p.DiscountPrice ?? p.Price) >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => (p.DiscountPrice ?? p.Price) <= maxPrice.Value);

            var projected = query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Price,
                p.DiscountPrice,
                p.StockQuantity,
                p.SellerId,
                p.CategoryId,
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0
            });

            if (minRating.HasValue) projected = projected.Where(p => p.AverageRating >= (double)minRating.Value);

            var total = await projected.CountAsync();
            var items = await projected
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new { p.Id, p.Name, p.Slug, p.Price, p.DiscountPrice, p.StockQuantity, p.SellerId, p.CategoryId, averageRating = Math.Round(p.AverageRating, 2) })
                .ToListAsync();
            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var p = await _db.Products
                .Include(x => x.Images)
                .Include(x => x.Reviews)
                .SingleOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();

            var averageRating = p.Reviews.Count == 0 ? 0 : Math.Round(p.Reviews.Average(r => r.Rating), 2);
            return Ok(new
            {
                p.Id,
                p.Name,
                p.Slug,
                p.Description,
                p.Price,
                p.DiscountPrice,
                p.StockQuantity,
                p.SellerId,
                p.CategoryId,
                p.IsFeatured,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt,
                images = p.Images,
                averageRating,
                totalReviews = p.Reviews.Count
            });
        }

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> Reviews(Guid id)
        {
            var reviews = await _db.ProductReviews
                .Where(r => r.ProductId == id)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.ProductId,
                    r.UserId,
                    userName = r.User.FullName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            var averageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);
            return Ok(new { averageRating, totalReviews = reviews.Count, reviews });
        }

        // Admin create endpoint (in real app protect with [Authorize(Roles = "Admin")])
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] dynamic model)
        {
            var sellerId = Guid.Parse((string)model.sellerId);
            var isApprovedSeller = await _db.SellerProfiles.AnyAsync(sp => sp.UserId == sellerId && sp.ApprovalStatus == Backend.Domain.SellerApprovalStatus.Approved);
            if (!isApprovedSeller) return BadRequest("Seller is not approved");

            var product = new Backend.Domain.Entities.Product
            {
                Id = Guid.NewGuid(),
                Name = (string)model.name,
                Slug = (string)model.slug,
                Price = (decimal)model.price,
                StockQuantity = (int)model.stockQuantity,
                SellerId = sellerId,
                CategoryId = Guid.Parse((string)model.categoryId),
                CreatedAt = DateTime.UtcNow
            };
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }
    }
}
