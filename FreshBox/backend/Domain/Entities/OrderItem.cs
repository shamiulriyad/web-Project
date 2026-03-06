using System;

namespace Backend.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid SellerId { get; set; }
        public Guid? PayoutId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal SellerEarning { get; set; }

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public User Seller { get; set; } = null!;
        public Payout? Payout { get; set; }
    }
}
