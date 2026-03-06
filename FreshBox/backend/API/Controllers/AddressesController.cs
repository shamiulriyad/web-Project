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
    [Route("api/addresses")]
    [Route("addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AddressesController(ApplicationDbContext db) => _db = db;

        [HttpPost]
        [CustomerOnly]
        public async Task<IActionResult> Create([FromBody] AddressRequest req)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId && u.IsActive);
            if (!userExists) return BadRequest("Invalid user");

            if (req.IsDefault)
            {
                var existingDefaults = await _db.UserAddresses.Where(a => a.UserId == req.UserId && a.IsDefault).ToListAsync();
                existingDefaults.ForEach(a => a.IsDefault = false);
            }

            var address = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                Name = req.Name,
                Phone = req.Phone,
                AddressLine = req.AddressLine,
                City = req.City,
                Division = req.City,
                District = req.City,
                Area = req.City,
                PostalCode = req.PostalCode,
                IsDefault = req.IsDefault,
                CreatedAt = DateTime.UtcNow
            };

            await _db.UserAddresses.AddAsync(address);
            await _db.SaveChangesAsync();
            return Ok(address);
        }

        [HttpGet]
        [CustomerOnly]
        public async Task<IActionResult> Get([FromQuery] Guid userId)
        {
            var addresses = await _db.UserAddresses.Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ToListAsync();
            return Ok(addresses);
        }

        [HttpPut("{id:guid}")]
        [CustomerOnly]
        public async Task<IActionResult> Update(Guid id, [FromBody] AddressRequest req)
        {
            var address = await _db.UserAddresses.SingleOrDefaultAsync(a => a.Id == id && a.UserId == req.UserId);
            if (address == null) return NotFound();

            if (req.IsDefault)
            {
                var existingDefaults = await _db.UserAddresses.Where(a => a.UserId == req.UserId && a.IsDefault && a.Id != id).ToListAsync();
                existingDefaults.ForEach(a => a.IsDefault = false);
            }

            address.Name = req.Name;
            address.Phone = req.Phone;
            address.AddressLine = req.AddressLine;
            address.City = req.City;
            address.Division = req.City;
            address.District = req.City;
            address.Area = req.City;
            address.PostalCode = req.PostalCode;
            address.IsDefault = req.IsDefault;

            await _db.SaveChangesAsync();
            return Ok(address);
        }

        [HttpDelete("{id:guid}")]
        [CustomerOnly]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid userId)
        {
            var address = await _db.UserAddresses.SingleOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (address == null) return NotFound();

            _db.UserAddresses.Remove(address);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}