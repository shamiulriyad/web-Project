using System;

namespace Backend.Domain.Entities
{
    public class CommissionRule
    {
        public Guid Id { get; set; }
        public Guid? SellerId { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal Percentage { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User? Seller { get; set; }
        public Category? Category { get; set; }
    }
}
