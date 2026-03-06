using System;

namespace Backend.Application.DTOs
{
    public record AddCartItemRequest(Guid UserId, Guid ProductId, int Quantity);
    public record UpdateCartItemRequest(Guid UserId, Guid ProductId, int Quantity);
    public record RemoveCartItemRequest(Guid UserId, Guid ProductId);

    public record AddressRequest(Guid UserId, string Name, string Phone, string AddressLine, string City, string? PostalCode, bool IsDefault);

    public record OrderStatusUpdateRequest(Backend.Domain.OrderStatus Status);

    public record CheckoutFromCartRequest(Guid UserId, Guid AddressId, Backend.Domain.PaymentMethod PaymentMethod, string? DiscountCode);

    public record CreateReviewRequest(Guid ProductId, Guid UserId, int Rating, string? Comment);
}