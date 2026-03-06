using System;
using Backend.Domain.Entities;

namespace Backend.Application.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, out DateTime expiresAt);
        string GenerateRefreshToken();
    }
}
