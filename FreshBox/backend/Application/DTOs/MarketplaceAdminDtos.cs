using System;

namespace Backend.Application.DTOs
{
    public record PayoutRequestDto(Guid SellerId);

    public record PayoutDecisionDto(Backend.Domain.PayoutStatus Status);

    public record CommissionRuleRequest(Guid? SellerId, Guid? CategoryId, decimal Percentage, bool IsActive = true);

    public record CreateCouponRequest(string Code, string Type, decimal Value, decimal? MinimumPurchaseAmount, int? UsageLimit, DateTime? ExpiryDate);

    public record MarkNotificationReadRequest(bool IsRead);

    public record UpdateStockRequest(int StockQuantity, int? LowStockAlertThreshold);
}