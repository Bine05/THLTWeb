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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Chiến thần" },
                new Category { Id = 2, Name = "Chiến tướng" },
                new Category { Id = 3, Name = "Tinh anh" },
                new Category { Id = 4, Name = "Kim cương" },
                new Category { Id = 5, Name = "Bạc kim" },
                new Category { Id = 6, Name = "Cao thủ" }
            );
        }
    }
}
