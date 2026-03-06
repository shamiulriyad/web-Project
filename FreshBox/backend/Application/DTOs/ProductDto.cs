using System;

namespace Backend.Application.DTOs
{
    public record ProductDto(Guid Id, string Name, string Slug, string? Description, decimal Price, decimal? DiscountPrice, int StockQuantity, Guid CategoryId);
}
