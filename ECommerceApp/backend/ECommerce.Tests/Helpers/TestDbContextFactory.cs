using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            
            // Seed test data
            SeedTestData(context);
            
            return context;
        }

        private static void SeedTestData(AppDbContext context)
        {
            // Seed Categories
            var categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Slug = "electronics",
                    Description = "Electronic devices",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    Slug = "clothing",
                    Description = "Clothes and fashion",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Categories.AddRange(categories);

            // Seed Products
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Test Product 1",
                    Slug = "test-product-1",
                    Description = "Test description",
                    Price = 99.99m,
                    StockQuantity = 10,
                    CategoryId = 1,
                    IsActive = true,
                    IsFeatured = true,
                    Rating = 4.5,
                    ReviewCount = 10,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Name = "Test Product 2",
                    Slug = "test-product-2",
                    Description = "Another test product",
                    Price = 49.99m,
                    StockQuantity = 5,
                    CategoryId = 2,
                    IsActive = true,
                    IsFeatured = false,
                    Rating = 4.0,
                    ReviewCount = 5,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(products);

            // Seed User
            var user = new User
            {
                Id = "test-user-id",
                UserName = "testuser@test.com",
                Email = "testuser@test.com",
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true,
                PhoneNumber = "1234567890",
                DateOfBirth = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(user);

            context.SaveChanges();
        }

        public static void Cleanup(AppDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}
