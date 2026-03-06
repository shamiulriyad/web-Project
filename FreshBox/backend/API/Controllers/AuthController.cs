using System;
using System.Threading.Tasks;
using Backend.Application.DTOs;
using Backend.Application.Services;
using Backend.Data;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IJwtService _jwt;

        public AuthController(ApplicationDbContext db, IJwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return BadRequest("Email already used");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = Backend.Domain.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateAccessToken(user, out var expiresAt);
            var refresh = _jwt.GenerateRefreshToken();

            // Persist refresh token
            _db.RefreshTokens.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = refresh, ExpiryDate = DateTime.UtcNow.AddDays(30), CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            return Ok(new AuthResponse(token, refresh, expiresAt));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Unauthorized("Invalid credentials");
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash)) return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateAccessToken(user, out var expiresAt);
            var refresh = _jwt.GenerateRefreshToken();

            _db.RefreshTokens.Add(new RefreshToken { Id = Guid.NewGuid(), UserId = user.Id, Token = refresh, ExpiryDate = DateTime.UtcNow.AddDays(30), CreatedAt = DateTime.UtcNow });
            await _db.SaveChangesAsync();

            return Ok(new AuthResponse(token, refresh, expiresAt));
        }
    }
}
