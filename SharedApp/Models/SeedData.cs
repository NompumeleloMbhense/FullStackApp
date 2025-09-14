using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SharedApp.Data;

namespace SharedApp.Models
{
    public static class SeedData
    {
        public static void EnsurePopulated(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Apply pending migrations
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }

            // Only seed if no suppliers exist
            if (!db.Suppliers.Any())
            {
                var supplier1 = new Supplier { Name = "Tech World", Location = "Roodepoort" };
                var supplier2 = new Supplier { Name = "Sound Co", Location = "Randburg" };

                db.Suppliers.AddRange(supplier1, supplier2);
                db.Products.AddRange(
                    new Product { Name = "Laptop", Price = 12000.50M, Stock = 25, Category = "Electronics", SupplierId = supplier1.SupplierId },
                    new Product { Name = "Headphones", Price = 450.00M, Stock = 100, Category = "Audio", SupplierId = supplier2.SupplierId }
                );

                db.SaveChanges();
            }
        }
    }
}