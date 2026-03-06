using System;
using System.Collections.Generic;

namespace Backend.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public Backend.Domain.UserRole Role { get; set; } = Backend.Domain.UserRole.User;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public Cart? Cart { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<DiscountUsage> DiscountUsages { get; set; } = new List<DiscountUsage>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public SellerProfile? SellerProfile { get; set; }
        public ICollection<Product> SellingProducts { get; set; } = new List<Product>();
        public ICollection<OrderItem> SellerOrderItems { get; set; } = new List<OrderItem>();
    }
}
