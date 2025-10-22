using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Models;
using WebGiayOnline.Models.Momo;

namespace WebGiayOnline.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Giay> Giays { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<GiaySize> GiaySizes { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<ProductGroup> ProductGroups { get; set; }

        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountGiay> DiscountGiays { get; set; }
        public DbSet<MomoInfoModel> MomoInfos { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
       
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Giay>()
                .HasOne(g => g.Brand)
                .WithMany(b => b.Giays)
                .HasForeignKey(g => g.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Giay>()
                .HasOne(g => g.Category)
                .WithMany(c => c.Giays)
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<GiaySize>()
     .HasKey(gs => new { gs.GiayId, gs.SizeId });

            modelBuilder.Entity<GiaySize>()
                .HasOne(gs => gs.Giay)
                .WithMany(g => g.GiaySizes)
                .HasForeignKey(gs => gs.GiayId);

            modelBuilder.Entity<GiaySize>()
                .HasOne(gs => gs.Size)
                .WithMany(s => s.GiaySizes)
                .HasForeignKey(gs => gs.SizeId);
            // Quan hệ ProductImage - Giay
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Giay)
                .WithMany(g => g.ProductImages)
                .HasForeignKey(pi => pi.GiayId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ ProductGroup - Giay
            modelBuilder.Entity<Giay>()
                .HasOne(g => g.ProductGroup)
                .WithMany(pg => pg.Variants)
                .HasForeignKey(g => g.ProductGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DiscountGiay>()
      .HasKey(dg => new { dg.DiscountId, dg.GiayId });

            modelBuilder.Entity<DiscountGiay>()
                .HasOne(dg => dg.Discount)
                .WithMany(d => d.DiscountGiays)
                .HasForeignKey(dg => dg.DiscountId);

            modelBuilder.Entity<DiscountGiay>()
                .HasOne(dg => dg.Giay)
                .WithMany(g => g.DiscountGiays)
                .HasForeignKey(dg => dg.GiayId);
            modelBuilder.Entity<ShoppingCart>()
       .HasOne(c => c.User)
       .WithMany() // Nếu bạn không muốn user có danh sách cart, thì để WithMany() rỗng
       .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<ShoppingCartItem>()
                .HasOne(i => i.ShoppingCart)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.ShoppingCartId);

            modelBuilder.Entity<ShoppingCartItem>()
                .HasOne(i => i.Giay)
                .WithMany() // Tùy nếu bạn muốn Product biết các ShoppingCartItem của nó
                .HasForeignKey(i => i.GiayId);

            modelBuilder.Entity<OrderStatusHistory>()
    .HasOne(h => h.Order)
    .WithMany(o => o.StatusHistories) // nếu bạn muốn tạo List trong Order
    .HasForeignKey(h => h.OrderId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
    .HasOne(r => r.OrderDetail)
    .WithMany()
    .HasForeignKey(r => r.OrderDetailId)
    .OnDelete(DeleteBehavior.Restrict);  // hoặc .NoAction

        }

    }
}
