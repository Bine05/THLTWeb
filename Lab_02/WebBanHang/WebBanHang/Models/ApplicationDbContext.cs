using Microsoft.EntityFrameworkCore;

namespace WebBanHang.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Laptop" },
                new Category { Id = 2, Name = "Desktop" },
                new Category { Id = 3, Name = "Điện thoại thông minh (Smartphone)" },
                new Category { Id = 4, Name = "Máy tính bảng (Tablet)" },
                new Category { Id = 5, Name = "Phụ kiện điện tử" },
                new Category { Id = 6, Name = "Đồng hồ thông minh (Smartwatch)" }
            );
        }
    }
}
