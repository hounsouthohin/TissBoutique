using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger? logger = null)
        {
            try
            {
                Log(logger, "🔄 Starting database initialization...");

                // Appliquer les migrations
                Log(logger, "📋 Applying migrations...");
                await context.Database.MigrateAsync();
                Log(logger, "✅ Migrations applied successfully");

                // Seeding des données
                await SeedRolesAsync(roleManager, logger);
                await SeedUsersAsync(userManager, logger);
                await SeedCategoriesAsync(context, logger);
                await SeedProductsAsync(context, logger);
                await SeedAddressesAsync(context, logger);
                await SeedReviewsAsync(context, logger);
                await SeedCartsAsync(context, logger);
                await SeedOrdersAsync(context, logger);

                Log(logger, "✅ Database initialization completed successfully!");
            }
            catch (Exception ex)
            {
                LogError(logger, $"❌ Error during database initialization: {ex.Message}");
                throw;
            }
        }

        private static void Log(ILogger? logger, string message)
        {
            logger?.LogInformation(message);
            Console.WriteLine(message);
        }

        private static void LogError(ILogger? logger, string message)
        {
            logger?.LogError(message);
            Console.WriteLine(message);
        }

        // ============================================
        // ROLES
        // ============================================
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger? logger)
        {
            Log(logger, "🔐 Seeding roles...");

            string[] roles = { "Admin", "Customer", "Seller" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        Log(logger, $"   ✅ Role '{role}' created");
                    }
                    else
                    {
                        LogError(logger, $"   ❌ Failed to create role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Log(logger, $"   ℹ️  Role '{role}' already exists");
                }
            }
        }

        // ============================================
        // USERS
        // ============================================
        private static async Task SeedUsersAsync(UserManager<User> userManager, ILogger? logger)
        {
            Log(logger, "👥 Seeding users...");

            var usersToSeed = new[]
            {
                new {
                    Email = "admin@ecommerce.com",
                    FirstName = "Admin",
                    LastName = "System",
                    Role = "Admin",
                    Password = "Admin123!",
                    Phone = "5141234567",
                    DateOfBirth = new DateTime(1985, 1, 1)
                },
                new {
                    Email = "john.doe@example.com",
                    FirstName = "John",
                    LastName = "Doe",
                    Role = "Customer",
                    Password = "Customer123!",
                    Phone = "5149876543",
                    DateOfBirth = new DateTime(1990, 5, 15)
                },
                new {
                    Email = "jane.smith@example.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Role = "Customer",
                    Password = "Customer123!",
                    Phone = "4385551234",
                    DateOfBirth = new DateTime(1992, 8, 20)
                },
                new {
                    Email = "seller@ecommerce.com",
                    FirstName = "Seller",
                    LastName = "Pro",
                    Role = "Seller",
                    Password = "Seller123!",
                    Phone = "4505556789",
                    DateOfBirth = new DateTime(1988, 3, 10)
                }
            };

            foreach (var u in usersToSeed)
            {
                if (await userManager.FindByEmailAsync(u.Email) == null)
                {
                    var user = new User
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        EmailConfirmed = true,
                        PhoneNumber = u.Phone,
                        DateOfBirth = DateTime.SpecifyKind(u.DateOfBirth, DateTimeKind.Utc),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var result = await userManager.CreateAsync(user, u.Password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, u.Role);
                        Log(logger, $"   ✅ User '{u.Email}' created with role '{u.Role}'");
                    }
                    else
                    {
                        LogError(logger, $"   ❌ Failed to create user '{u.Email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Log(logger, $"   ℹ️  User '{u.Email}' already exists");
                }
            }
        }

        // ============================================
        // CATEGORIES
        // ============================================
        private static async Task SeedCategoriesAsync(AppDbContext context, ILogger? logger)
        {
            Log(logger, "📁 Seeding categories...");

            if (await context.Categories.AnyAsync())
            {
                Log(logger, "   ℹ️  Categories already exist, skipping...");
                return;
            }

            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Electronics",
                    Slug = "electronics",
                    Description = "Latest electronic devices, gadgets, and accessories",
                    ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Clothing",
                    Slug = "clothing",
                    Description = "Fashion for men, women, and children",
                    ImageUrl = "https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Home & Garden",
                    Slug = "home-garden",
                    Description = "Everything for your home and garden",
                    ImageUrl = "https://images.unsplash.com/photo-1484101403633-562f891dc89a?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Books",
                    Slug = "books",
                    Description = "Books, magazines, and educational materials",
                    ImageUrl = "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Sports & Outdoors",
                    Slug = "sports-outdoors",
                    Description = "Sports equipment, outdoor gear, and fitness accessories",
                    ImageUrl = "https://images.unsplash.com/photo-1461896836934-ffe607ba8211?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Beauty & Health",
                    Slug = "beauty-health",
                    Description = "Beauty products, skincare, and health essentials",
                    ImageUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=400",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            Log(logger, $"   ✅ Seeded {categories.Count} categories");
        }

        // ============================================
        // PRODUCTS
        // ============================================
        private static async Task SeedProductsAsync(AppDbContext context, ILogger? logger)
        {
            Log(logger, "📦 Seeding products...");

            if (await context.Products.AnyAsync())
            {
                Log(logger, "   ℹ️  Products already exist, skipping...");
                return;
            }

            var categories = await context.Categories.ToListAsync();
            if (!categories.Any())
            {
                LogError(logger, "   ❌ No categories found, cannot seed products");
                return;
            }

            var electronics = categories.FirstOrDefault(c => c.Slug == "electronics");
            var clothing = categories.FirstOrDefault(c => c.Slug == "clothing");
            var homeGarden = categories.FirstOrDefault(c => c.Slug == "home-garden");
            var books = categories.FirstOrDefault(c => c.Slug == "books");
            var sports = categories.FirstOrDefault(c => c.Slug == "sports-outdoors");
            var beauty = categories.FirstOrDefault(c => c.Slug == "beauty-health");

            var products = new List<Product>();

            // ELECTRONICS
            if (electronics != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Wireless Bluetooth Headphones",
                        Slug = "wireless-bluetooth-headphones",
                        Description = "Premium wireless headphones with active noise cancellation, 30-hour battery life, and superior sound quality.",
                        Price = 89.99m,
                        CompareAtPrice = 129.99m,
                        StockQuantity = 50,
                        Sku = "WBH-001",
                        CategoryId = electronics.Id,
                        IsActive = true,
                        IsFeatured = true,
                        Rating = 4.5,
                        ReviewCount = 120,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    },
                    new Product
                    {
                        Name = "Smart Watch Series 5",
                        Slug = "smart-watch-series-5",
                        Description = "Advanced fitness tracker with heart rate monitor, GPS, sleep tracking, and water resistance.",
                        Price = 249.99m,
                        CompareAtPrice = 299.99m,
                        StockQuantity = 30,
                        Sku = "SW-005",
                        CategoryId = electronics.Id,
                        IsActive = true,
                        IsFeatured = true,
                        Rating = 4.8,
                        ReviewCount = 89,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    },
                    new Product
                    {
                        Name = "4K Ultra HD Webcam",
                        Slug = "4k-ultra-hd-webcam",
                        Description = "Professional webcam with 4K resolution, auto-focus, and built-in dual microphones.",
                        Price = 79.99m,
                        StockQuantity = 45,
                        Sku = "WC-4K-001",
                        CategoryId = electronics.Id,
                        IsActive = true,
                        IsFeatured = false,
                        Rating = 4.3,
                        ReviewCount = 67,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    }
                });
            }

            // CLOTHING
            if (clothing != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Premium Cotton T-Shirt",
                        Slug = "premium-cotton-t-shirt",
                        Description = "100% organic cotton t-shirt with a comfortable fit. Eco-friendly and breathable fabric.",
                        Price = 19.99m,
                        CompareAtPrice = 29.99m,
                        StockQuantity = 200,
                        Sku = "TS-100",
                        CategoryId = clothing.Id,
                        IsActive = true,
                        IsFeatured = false,
                        Rating = 4.2,
                        ReviewCount = 45,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    },
                    new Product
                    {
                        Name = "Classic Denim Jeans",
                        Slug = "classic-denim-jeans",
                        Description = "Timeless denim jeans with a perfect fit. Durable, comfortable, and stylish.",
                        Price = 49.99m,
                        CompareAtPrice = 69.99m,
                        StockQuantity = 100,
                        Sku = "DJ-200",
                        CategoryId = clothing.Id,
                        IsActive = true,
                        IsFeatured = true,
                        Rating = 4.6,
                        ReviewCount = 78,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    },
                    new Product
                    {
                        Name = "Winter Jacket",
                        Slug = "winter-jacket",
                        Description = "Warm and waterproof winter jacket with down insulation. Perfect for cold Canadian winters.",
                        Price = 129.99m,
                        CompareAtPrice = 179.99m,
                        StockQuantity = 60,
                        Sku = "WJ-300",
                        CategoryId = clothing.Id,
                        IsActive = true,
                        IsFeatured = true,
                        Rating = 4.7,
                        ReviewCount = 92,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1539533018447-63fcce2678e3?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    }
                });
            }

            // HOME & GARDEN
            if (homeGarden != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "LED Desk Lamp",
                        Slug = "led-desk-lamp",
                        Description = "Modern LED desk lamp with adjustable brightness and color temperature.",
                        Price = 34.99m,
                        StockQuantity = 75,
                        Sku = "LAMP-001",
                        CategoryId = homeGarden.Id,
                        IsActive = true,
                        IsFeatured = false,
                        Rating = 4.3,
                        ReviewCount = 56,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    },
                    new Product
                    {
                        Name = "Ceramic Cookware Set",
                        Slug = "ceramic-cookware-set",
                        Description = "10-piece non-stick ceramic cookware set. Healthy cooking with even heat distribution.",
                        Price = 89.99m,
                        CompareAtPrice = 129.99m,
                        StockQuantity = 40,
                        Sku = "CWS-500",
                        CategoryId = homeGarden.Id,
                        IsActive = true,
                        IsFeatured = true,
                        Rating = 4.5,
                        ReviewCount = 103,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    }
                });
            }

            // BOOKS
            if (books != null)
            {
                products.Add(new Product
                {
                    Name = "The Art of Programming",
                    Slug = "the-art-of-programming",
                    Description = "Comprehensive guide to modern programming practices.",
                    Price = 39.99m,
                    CompareAtPrice = 49.99m,
                    StockQuantity = 150,
                    Sku = "BK-PROG-001",
                    CategoryId = books.Id,
                    IsActive = true,
                    IsFeatured = true,
                    Rating = 4.8,
                    ReviewCount = 234,
                    CreatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                    }
                });
            }

            // SPORTS
            if (sports != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Yoga Mat Pro",
                        Slug = "yoga-mat-pro",
                        Description = "Premium non-slip yoga mat with extra cushioning.",
                        Price = 29.99m,
                        StockQuantity = 120,
                        Sku = "YM-PRO-001",
                        CategoryId = sports.Id,
                        IsActive = true,
                        IsFeatured = false,
                        Rating = 4.4,
                        ReviewCount = 87,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1601925260368-ae2f83cf8b7f?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    },
                    new Product
                    {
                        Name = "Adjustable Dumbbells Set",
                        Slug = "adjustable-dumbbells-set",
                        Description = "Space-saving adjustable dumbbells from 5 to 25 lbs.",
                        Price = 149.99m,
                        CompareAtPrice = 199.99m,
                        StockQuantity = 35,
                        Sku = "DB-ADJ-001",
                        CategoryId = sports.Id,
                        IsActive = true,
                        IsFeatured = true,
                        Rating = 4.7,
                        ReviewCount = 156,
                        CreatedAt = DateTime.UtcNow,
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1638805616366-e12c1d7be48f?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                        }
                    }
                });
            }

            // BEAUTY
            if (beauty != null)
            {
                products.Add(new Product
                {
                    Name = "Natural Face Serum",
                    Slug = "natural-face-serum",
                    Description = "Organic face serum with vitamin C and hyaluronic acid.",
                    Price = 44.99m,
                    CompareAtPrice = 59.99m,
                    StockQuantity = 80,
                    Sku = "FS-NAT-001",
                    CategoryId = beauty.Id,
                    IsActive = true,
                    IsFeatured = true,
                    Rating = 4.6,
                    ReviewCount = 198,
                    CreatedAt = DateTime.UtcNow,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = "https://images.unsplash.com/photo-1620916566398-39f1143ab7be?w=600", IsPrimary = true, DisplayOrder = 0, CreatedAt = DateTime.UtcNow }
                    }
                });
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            Log(logger, $"   ✅ Seeded {products.Count} products with images");
        }

        // ============================================
        // ADDRESSES
        // ============================================
        private static async Task SeedAddressesAsync(AppDbContext context, ILogger? logger)
        {
            Log(logger, "📍 Seeding addresses...");

            if (await context.Addresses.AnyAsync())
            {
                Log(logger, "   ℹ️  Addresses already exist, skipping...");
                return;
            }

            var users = await context.Users.ToListAsync();
            if (!users.Any())
            {
                LogError(logger, "   ❌ No users found, cannot seed addresses");
                return;
            }

            var johnDoe = users.FirstOrDefault(u => u.Email == "john.doe@example.com");
            var janeSmith = users.FirstOrDefault(u => u.Email == "jane.smith@example.com");

            var addresses = new List<Address>();

            if (johnDoe != null)
            {
                addresses.AddRange(new[]
                {
                    new Address
                    {
                        UserId = johnDoe.Id,
                        Street = "123 Rue Saint-Catherine",
                        City = "Montreal",
                        Province = "QC",
                        PostalCode = "H3B 1A1",
                        Country = "Canada",
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Address
                    {
                        UserId = johnDoe.Id,
                        Street = "456 Avenue du Parc",
                        City = "Montreal",
                        Province = "QC",
                        PostalCode = "H2V 4E5",
                        Country = "Canada",
                        IsDefault = false,
                        CreatedAt = DateTime.UtcNow
                    }
                });
            }

            if (janeSmith != null)
            {
                addresses.Add(new Address
                {
                    UserId = janeSmith.Id,
                    Street = "789 Boulevard René-Lévesque",
                    City = "Quebec City",
                    Province = "QC",
                    PostalCode = "G1R 5B1",
                    Country = "Canada",
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Addresses.AddRangeAsync(addresses);
            await context.SaveChangesAsync();
            Log(logger, $"   ✅ Seeded {addresses.Count} addresses");
        }

        // ============================================
        // REVIEWS
        // ============================================
        private static async Task SeedReviewsAsync(AppDbContext context, ILogger? logger)
        {
            Log(logger, "⭐ Seeding reviews...");

            if (await context.Reviews.AnyAsync())
            {
                Log(logger, "   ℹ️  Reviews already exist, skipping...");
                return;
            }

            var users = await context.Users.ToListAsync();
            var products = await context.Products.ToListAsync();

            if (!users.Any() || !products.Any())
            {
                LogError(logger, "   ❌ No users or products found, cannot seed reviews");
                return;
            }

            var johnDoe = users.FirstOrDefault(u => u.Email == "john.doe@example.com");
            var janeSmith = users.FirstOrDefault(u => u.Email == "jane.smith@example.com");
            var headphones = products.FirstOrDefault(p => p.Slug == "wireless-bluetooth-headphones");
            var smartWatch = products.FirstOrDefault(p => p.Slug == "smart-watch-series-5");
            var jeans = products.FirstOrDefault(p => p.Slug == "classic-denim-jeans");

            var reviews = new List<Review>();

            if (headphones != null && johnDoe != null)
            {
                reviews.Add(new Review
                {
                    ProductId = headphones.Id,
                    UserId = johnDoe.Id,
                    Rating = 5,
                    Title = "Excellent sound quality!",
                    Comment = "These headphones are amazing! The noise cancellation works perfectly and the battery lasts all day.",
                    IsVerifiedPurchase = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                });
            }

            if (smartWatch != null && janeSmith != null)
            {
                reviews.Add(new Review
                {
                    ProductId = smartWatch.Id,
                    UserId = janeSmith.Id,
                    Rating = 4,
                    Title = "Great fitness tracker",
                    Comment = "Love the fitness tracking features. GPS is accurate and battery life is good.",
                    IsVerifiedPurchase = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                });
            }

            if (jeans != null && johnDoe != null)
            {
                reviews.Add(new Review
                {
                    ProductId = jeans.Id,
                    UserId = johnDoe.Id,
                    Rating = 5,
                    Title = "Perfect fit!",
                    Comment = "These jeans fit perfectly and are very comfortable.",
                    IsVerifiedPurchase = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                });
            }

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
            Log(logger, $"   ✅ Seeded {reviews.Count} reviews");
        }

        // ============================================
        // CARTS
        // ============================================
        private static async Task SeedCartsAsync(AppDbContext context, ILogger? logger)
        {
            Log(logger, "🛒 Seeding carts...");

            if (await context.Carts.AnyAsync())
            {
                Log(logger, "   ℹ️  Carts already exist, skipping...");
                return;
            }

            var users = await context.Users.ToListAsync();
            var products = await context.Products.ToListAsync();

            var janeSmith = users.FirstOrDefault(u => u.Email == "jane.smith@example.com");

            if (janeSmith != null && products.Any())
            {
                var tshirt = products.FirstOrDefault(p => p.Slug == "premium-cotton-t-shirt");
                var lamp = products.FirstOrDefault(p => p.Slug == "led-desk-lamp");

                var cart = new Cart
                {
                    UserId = janeSmith.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };

                if (tshirt != null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = tshirt.Id,
                        Quantity = 2,
                        UnitPrice = tshirt.Price,
                        AddedAt = DateTime.UtcNow.AddDays(-2)
                    });
                }

                if (lamp != null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = lamp.Id,
                        Quantity = 1,
                        UnitPrice = lamp.Price,
                        AddedAt = DateTime.UtcNow.AddDays(-1)
                    });
                }

                await context.Carts.AddAsync(cart);
                await context.SaveChangesAsync();
                Log(logger, $"   ✅ Seeded 1 cart with {cart.Items.Count} items");
            }
            else
            {
                LogError(logger, "   ❌ Required user or products not found");
            }
        }

        // ============================================
        // ORDERS
        // ============================================
        private static async Task SeedOrdersAsync(AppDbContext context, ILogger? logger)
        {
            Log(logger, "📦 Seeding orders...");

            if (await context.Orders.AnyAsync())
            {
                Log(logger, "   ℹ️  Orders already exist, skipping...");
                return;
            }

            var users = await context.Users.ToListAsync();
            var products = await context.Products.ToListAsync();

            var johnDoe = users.FirstOrDefault(u => u.Email == "john.doe@example.com");

            if (johnDoe != null && products.Any())
            {
                var headphones = products.FirstOrDefault(p => p.Slug == "wireless-bluetooth-headphones");
                var jeans = products.FirstOrDefault(p => p.Slug == "classic-denim-jeans");

                if (headphones != null && jeans != null)
                {
                    var order = new Order
                    {
                        OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-0001",
                        UserId = johnDoe.Id,
                        Status = OrderStatus.Delivered,
                        ShippingStreet = "123 Rue Saint-Catherine",
                        ShippingCity = "Montreal",
                        ShippingProvince = "QC",
                        ShippingPostalCode = "H3B 1A1",
                        ShippingCountry = "Canada",
                        ShippingAmount = 10.00m,
                        TrackingNumber = "1Z999AA10123456784",
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        UpdatedAt = DateTime.UtcNow.AddDays(-15),
                        ShippedAt = DateTime.UtcNow.AddDays(-18),
                        DeliveredAt = DateTime.UtcNow.AddDays(-15),
                        Items = new List<OrderItem>
                        {
                            new OrderItem
                            {
                                ProductId = headphones.Id,
                                ProductName = headphones.Name,
                                Quantity = 1,
                                UnitPrice = headphones.Price,
                                Discount = 0
                            },
                            new OrderItem
                            {
                                ProductId = jeans.Id,
                                ProductName = jeans.Name,
                                Quantity = 2,
                                UnitPrice = jeans.Price,
                                Discount = 10.00m
                            }
                        }
                    };

                    order.CalculateTotals();

                    order.Payment = new Payment
                    {
                        StripePaymentIntentId = "pi_test_" + Guid.NewGuid().ToString()[..16],
                        Amount = order.TotalAmount,
                        Currency = "CAD",
                        Status = PaymentStatus.Completed,
                        PaymentMethod = "card",
                        CreatedAt = order.CreatedAt,
                        CompletedAt = order.CreatedAt.AddMinutes(2)
                    };

                    await context.Orders.AddAsync(order);
                    await context.SaveChangesAsync();
                    Log(logger, $"   ✅ Seeded 1 order with {order.Items.Count} items");
                }
                else
                {
                    LogError(logger, "   ❌ Required products not found");
                }
            }
            else
            {
                LogError(logger, "   ❌ Required user or products not found");
            }
        }
    }
}