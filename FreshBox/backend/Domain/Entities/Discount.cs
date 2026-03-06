using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities
{
    public class Discount
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Type { get; set; } = null!; // Percentage / Fixed
        public decimal Value { get; set; }
        public decimal? MinimumPurchaseAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public ICollection<DiscountUsage> Usages { get; set; } = new List<DiscountUsage>();
    }
}
