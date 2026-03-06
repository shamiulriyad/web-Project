using System;

namespace Backend.Domain.Entities
{
    public class ProductReview
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid UserId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
