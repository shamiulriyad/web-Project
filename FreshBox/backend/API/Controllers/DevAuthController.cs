using System;
using Backend.Application.Services;
using Backend.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevAuthController : ControllerBase
    {
        private readonly IJwtService _jwt;

        public DevAuthController(IJwtService jwt)
        {
            _jwt = jwt;
        }

        // Development helper: generate a JWT for quick testing in Swagger
        // Usage: GET /api/devauth/token?role=Admin
        [HttpGet("token")]
        public IActionResult GetToken([FromQuery] string role = "User")
        {
            // Only intended for development/testing
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = $"Dev {role}",
                Email = role.ToLower() + "@local.dev",
                Role = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? Backend.Domain.UserRole.Admin : Backend.Domain.UserRole.User,
                CreatedAt = DateTime.UtcNow
            };

            var token = _jwt.GenerateAccessToken(user, out var expiresAt);
            return Ok(new { token, expiresAt });
        }
    }
}
