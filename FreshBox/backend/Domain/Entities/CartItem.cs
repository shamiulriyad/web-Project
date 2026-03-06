using System;

namespace Backend.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }

        public Cart Cart { get; set; } = null!;
        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
