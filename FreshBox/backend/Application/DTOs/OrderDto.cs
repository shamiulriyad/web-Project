using System;
using System.Collections.Generic;

namespace Backend.Application.DTOs
{
    public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);
    public record CreateOrderDto(Guid UserId, Guid AddressId, Backend.Domain.PaymentMethod PaymentMethod, IEnumerable<OrderItemDto> Items, string? DiscountCode);
}
