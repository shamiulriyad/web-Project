using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Application.DTOs;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public NotificationsController(ApplicationDbContext db) => _db = db;

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            var items = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPatch("{notificationId:guid}")]
        public async Task<IActionResult> Mark(Guid notificationId, [FromBody] MarkNotificationReadRequest req)
        {
            var item = await _db.Notifications.SingleOrDefaultAsync(n => n.Id == notificationId);
            if (item == null) return NotFound();
            item.IsRead = req.IsRead;
            await _db.SaveChangesAsync();
            return Ok(item);
        }
    }
}