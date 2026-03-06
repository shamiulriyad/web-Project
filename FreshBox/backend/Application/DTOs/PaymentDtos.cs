using System;

namespace Backend.Application.DTOs
{
    public record SubmitPaymentRequest(Guid OrderId, Guid UserId, decimal Amount, Backend.Domain.PaymentMethod PaymentMethod, string TransactionId);

    public record VerifyPaymentRequest(Backend.Domain.PaymentStatus Status);

    public record VerifyPaymentAdminRequest(Guid PaymentId, Backend.Domain.PaymentStatus Status);

    public record UpdateOrderStatusRequest(Backend.Domain.OrderStatus OrderStatus);
}