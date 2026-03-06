using System;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public WishlistController(ApplicationDbContext db) => _db = db;

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            var items = await _db.Wishlists.Include(w => w.Product).Where(w => w.UserId == userId).ToListAsync();
            return Ok(items);
        }
    }
}
