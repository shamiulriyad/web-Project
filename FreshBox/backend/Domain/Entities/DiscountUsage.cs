using System;

namespace Backend.Domain.Entities
{
    public class DiscountUsage
    {
        public Guid Id { get; set; }
        public Guid DiscountId { get; set; }
        public Guid UserId { get; set; }
        public DateTime UsedAt { get; set; }

        public Discount Discount { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
