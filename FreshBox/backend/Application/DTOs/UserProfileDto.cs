using System;

namespace Backend.Application.DTOs
{
    public record UserProfileDto(Guid Id, string FullName, string Email, string? PhoneNumber);
}
