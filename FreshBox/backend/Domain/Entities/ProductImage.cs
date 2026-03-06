using System;

namespace Backend.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; } = null!;
    }
}
