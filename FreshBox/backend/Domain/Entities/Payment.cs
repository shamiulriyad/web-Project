using System;

namespace Backend.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public Backend.Domain.PaymentMethod PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public Backend.Domain.PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaymentDate { get; set; }

        public Order Order { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
