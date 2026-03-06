using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities
{
    public class Payout
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public decimal Amount { get; set; }
        public Backend.Domain.PayoutStatus Status { get; set; } = Backend.Domain.PayoutStatus.Requested;
        public DateTime RequestedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public User Seller { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
