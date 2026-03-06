using System;

namespace Backend.Domain.Entities
{
    public class SellerProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ShopName { get; set; } = null!;
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? District { get; set; }
        public string? Upazila { get; set; }
        public Backend.Domain.SellerApprovalStatus ApprovalStatus { get; set; } = Backend.Domain.SellerApprovalStatus.Pending;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
