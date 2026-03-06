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
    [Route("api/[controller]")]
    [Route("reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ReviewsController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        [CustomerOnly]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest req)
        {
            if (req.Rating < 1 || req.Rating > 5) return BadRequest("Rating must be between 1 and 5");

            var productExists = await _db.Products.AnyAsync(p => p.Id == req.ProductId && p.IsActive);
            if (!productExists) return BadRequest("Invalid product");

            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId && u.IsActive);
            if (!userExists) return BadRequest("Invalid user");

            var review = await _db.ProductReviews.SingleOrDefaultAsync(r => r.ProductId == req.ProductId && r.UserId == req.UserId);
            if (review != null)
            {
                review.Rating = req.Rating;
                review.Comment = req.Comment;
                await _db.SaveChangesAsync();
                return Ok(review);
            }

            review = new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = req.ProductId,
                UserId = req.UserId,
                Rating = req.Rating,
                Comment = req.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _db.ProductReviews.AddAsync(review);
            await _db.SaveChangesAsync();
            return Ok(review);
        }

        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> ProductReviews(Guid productId)
        {
            var reviews = await _db.ProductReviews
                .Where(r => r.ProductId == productId)
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

        [HttpGet("seller/{sellerId:guid}")]
        public async Task<IActionResult> SellerRating(Guid sellerId)
        {
            var sellerProductIds = await _db.Products.Where(p => p.SellerId == sellerId).Select(p => p.Id).ToListAsync();
            var ratings = await _db.ProductReviews.Where(r => sellerProductIds.Contains(r.ProductId)).Select(r => r.Rating).ToListAsync();

            var averageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 2);
            return Ok(new { sellerId, averageRating, totalReviews = ratings.Count });
        }
    }
}