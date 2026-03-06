using System;

namespace Backend.Application.DTOs
{
    public record SellerApplicationRequest(Guid UserId, string ShopName, string? Bio, string? PhoneNumber, string? District, string? Upazila);

    public record SellerApprovalRequest(Backend.Domain.SellerApprovalStatus ApprovalStatus);
}