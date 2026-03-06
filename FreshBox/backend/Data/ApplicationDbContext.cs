using System;
using Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<DiscountUsage> DiscountUsages => Set<DiscountUsage>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
        public DbSet<Payout> Payouts => Set<Payout>();
        public DbSet<CommissionRule> CommissionRules => Set<CommissionRule>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b => {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).IsRequired();
                b.Property(u => u.FullName).IsRequired();
            });

            modelBuilder.Entity<UserAddress>(b => {
                b.HasKey(a => a.Id);
                b.HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cart>(b => {
                b.HasKey(c => c.Id);
                b.HasOne(c => c.User).WithOne(u => u.Cart).HasForeignKey<Cart>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(c => c.UserId).IsUnique();
            });

            modelBuilder.Entity<Category>(b => {
                b.HasKey(c => c.Id);
                b.HasIndex(c => c.Slug).IsUnique();
            });

            modelBuilder.Entity<Product>(b => {
                b.HasKey(p => p.Id);
                b.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
                b.HasOne(p => p.Seller).WithMany(u => u.SellingProducts).HasForeignKey(p => p.SellerId);
                b.HasIndex(p => p.Slug).IsUnique();
                b.HasIndex(p => p.SellerId);
            });

            modelBuilder.Entity<ProductImage>(b => {
                b.HasKey(pi => pi.Id);
                b.HasOne(pi => pi.Product).WithMany(p => p.Images).HasForeignKey(pi => pi.ProductId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CartItem>(b => {
                b.HasKey(ci => ci.Id);
                b.HasOne(ci => ci.Cart).WithMany(c => c.Items).HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(ci => ci.User).WithMany(u => u.CartItems).HasForeignKey(ci => ci.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(ci => ci.Product).WithMany(p => p.CartItems).HasForeignKey(ci => ci.ProductId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
            });

            modelBuilder.Entity<Wishlist>(b => {
                b.HasKey(w => w.Id);
                b.HasOne(w => w.User).WithMany(u => u.Wishlists).HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(w => w.Product).WithMany(p => p.Wishlists).HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
            });

            modelBuilder.Entity<Order>(b => {
                b.HasKey(o => o.Id);
                b.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
                b.HasOne(o => o.Address).WithMany().HasForeignKey(o => o.AddressId);
            });

            modelBuilder.Entity<OrderItem>(b => {
                b.HasKey(oi => oi.Id);
                b.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(oi => oi.Product).WithMany(p => p.OrderItems).HasForeignKey(oi => oi.ProductId);
                b.HasOne(oi => oi.Seller).WithMany(u => u.SellerOrderItems).HasForeignKey(oi => oi.SellerId);
                b.HasOne(oi => oi.Payout).WithMany(p => p.Items).HasForeignKey(oi => oi.PayoutId);
                b.HasIndex(oi => oi.SellerId);
            });

            modelBuilder.Entity<Payment>(b => {
                b.HasKey(p => p.Id);
                b.HasOne(p => p.Order).WithOne(o => o.Payment).HasForeignKey<Payment>(p => p.OrderId);
                b.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId);
                b.HasIndex(p => p.TransactionId).IsUnique();
                b.HasIndex(p => p.UserId);
            });

            modelBuilder.Entity<Discount>(b => {
                b.HasKey(d => d.Id);
                b.HasIndex(d => d.Code).IsUnique();
            });

            modelBuilder.Entity<DiscountUsage>(b => {
                b.HasKey(du => du.Id);
                b.HasOne(du => du.Discount).WithMany(d => d.Usages).HasForeignKey(du => du.DiscountId);
                b.HasOne(du => du.User).WithMany(u => u.DiscountUsages).HasForeignKey(du => du.UserId);
            });

            modelBuilder.Entity<RefreshToken>(b => {
                b.HasKey(rt => rt.Id);
                b.HasOne(rt => rt.User).WithMany(u => u.RefreshTokens).HasForeignKey(rt => rt.UserId);
            });

            modelBuilder.Entity<ProductReview>(b => {
                b.HasKey(r => r.Id);
                b.HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId);
                b.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
                b.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
            });

            modelBuilder.Entity<SellerProfile>(b => {
                b.HasKey(sp => sp.Id);
                b.HasOne(sp => sp.User).WithOne(u => u.SellerProfile).HasForeignKey<SellerProfile>(sp => sp.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(sp => sp.UserId).IsUnique();
                b.HasIndex(sp => sp.ApprovalStatus);
                b.Property(sp => sp.ShopName).IsRequired();
            });

            modelBuilder.Entity<Payout>(b => {
                b.HasKey(p => p.Id);
                b.HasOne(p => p.Seller).WithMany().HasForeignKey(p => p.SellerId);
                b.HasIndex(p => new { p.SellerId, p.Status });
            });

            modelBuilder.Entity<CommissionRule>(b => {
                b.HasKey(cr => cr.Id);
                b.HasOne(cr => cr.Seller).WithMany().HasForeignKey(cr => cr.SellerId);
                b.HasOne(cr => cr.Category).WithMany().HasForeignKey(cr => cr.CategoryId);
                b.HasIndex(cr => new { cr.SellerId, cr.CategoryId, cr.IsActive });
            });

            modelBuilder.Entity<Notification>(b => {
                b.HasKey(n => n.Id);
                b.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            });
        }
    }
}
