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
    [Route("api/cart")]
    [Route("cart")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CartController(ApplicationDbContext db) => _db = db;

        [HttpPost("add")]
        [CustomerOnly]
        public async Task<IActionResult> Add([FromBody] AddCartItemRequest req)
        {
            if (req.Quantity <= 0) return BadRequest("Quantity must be greater than 0");

            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId && u.IsActive);
            if (!userExists) return BadRequest("Invalid user");

            var product = await _db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == req.ProductId && p.IsActive);
            if (product == null) return BadRequest("Invalid product");

            var cart = await GetOrCreateCart(req.UserId);

            var existing = await _db.CartItems.SingleOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == req.ProductId);
            if (existing != null)
            {
                existing.Quantity += req.Quantity;
                await _db.SaveChangesAsync();
                return Ok(existing);
            }

            var item = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                UserId = req.UserId,
                ProductId = req.ProductId,
                Quantity = req.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            await _db.CartItems.AddAsync(item);
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpGet]
        [CustomerOnly]
        public async Task<IActionResult> Get([FromQuery] Guid userId)
        {
            var cart = await _db.Carts.AsNoTracking().SingleOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return Ok(new { userId, items = Array.Empty<object>(), totalAmount = 0m });

            var items = await _db.CartItems
                .Where(c => c.CartId == cart.Id)
                .Include(c => c.Product)
                .Select(c => new
                {
                    c.ProductId,
                    productName = c.Product.Name,
                    c.Quantity,
                    unitPrice = c.Product.DiscountPrice ?? c.Product.Price,
                    totalPrice = (c.Product.DiscountPrice ?? c.Product.Price) * c.Quantity
                })
                .ToListAsync();

            var totalAmount = items.Sum(i => i.totalPrice);
            return Ok(new { userId, items, totalAmount });
        }

        [HttpPut("update")]
        [CustomerOnly]
        public async Task<IActionResult> Update([FromBody] UpdateCartItemRequest req)
        {
            var cart = await _db.Carts.SingleOrDefaultAsync(c => c.UserId == req.UserId);
            if (cart == null) return NotFound("Cart not found");

            var item = await _db.CartItems.SingleOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == req.ProductId);
            if (item == null) return NotFound("Cart item not found");

            if (req.Quantity <= 0)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
                return Ok();
            }

            item.Quantity = req.Quantity;
            await _db.SaveChangesAsync();
            return Ok(item);
        }

        [HttpDelete("remove")]
        [CustomerOnly]
        public async Task<IActionResult> Remove([FromBody] RemoveCartItemRequest req)
        {
            var cart = await _db.Carts.SingleOrDefaultAsync(c => c.UserId == req.UserId);
            if (cart == null) return NotFound("Cart not found");

            var item = await _db.CartItems.SingleOrDefaultAsync(c => c.CartId == cart.Id && c.ProductId == req.ProductId);
            if (item == null) return NotFound("Cart item not found");

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return Ok();
        }

        private async Task<Cart> GetOrCreateCart(Guid userId)
        {
            var cart = await _db.Carts.SingleOrDefaultAsync(c => c.UserId == userId);
            if (cart != null) return cart;

            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Carts.AddAsync(cart);
            await _db.SaveChangesAsync();
            return cart;
        }
    }
}
