using System;

namespace Backend.Application.DTOs
{
    public record CategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl);
}
