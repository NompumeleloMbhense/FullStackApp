using Microsoft.EntityFrameworkCore;
using SharedApp.Models;

namespace ServerApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { SupplierId = 1, Name = "Tech World", Location = "Roodepoort" },
                new Supplier { SupplierId = 2, Name = "Sound Co", Location = "Ranburg" }
            );

            //Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Laptop",
                    Price = 12000.50M,
                    Stock = 25,
                    Category = "Electronics",
                    SupplierId = 1
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Headphones",
                    Price = 450.00M,
                    Stock = 100,
                    Category = "Audio",
                    SupplierId = 2
                }
            );
        }
    }
}