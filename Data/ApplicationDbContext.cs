using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using vinted2.Models;

namespace vinted2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Favorites
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

                entity.HasOne(f => f.Product)
                      .WithMany(p => p.Favorites)
                      .HasForeignKey(f => f.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // 🔹 schimbat de la Cascade la Restrict

                entity.HasOne(f => f.User)
                      .WithMany(u => u.Favorites)
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.Product)
                      .WithMany(p => p.Orders)
                      .HasForeignKey(o => o.ProductId)
                      .OnDelete(DeleteBehavior.SetNull); // Allow product deletion, set Product navigation to null

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ProductImages (dacă există)
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // previne multiple cascade paths
            });
        }
    }
}
