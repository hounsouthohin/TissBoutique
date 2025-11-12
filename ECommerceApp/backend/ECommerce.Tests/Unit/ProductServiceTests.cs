using AutoMapper;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Mappings;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Tests.Helpers;
using Xunit;

namespace ECommerce.Tests.Unit
{
    public class ProductServiceTests : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            var context = TestDbContextFactory.CreateInMemoryContext();
            
            // Setup AutoMapper
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = config.CreateMapper();

            // Setup UnitOfWork
            _unitOfWork = new UnitOfWork(context);

            // Create service
            _productService = new ProductService(_unitOfWork, _mapper);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
        {
            // Arrange
            int productId = 1;

            // Act
            var result = await _productService.GetByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("Test Product 1", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingProduct_ReturnsNull()
        {
            // Arrange
            int nonExistingId = 999;

            // Act
            var result = await _productService.GetByIdAsync(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBySlugAsync_ExistingSlug_ReturnsProduct()
        {
            // Arrange
            string slug = "test-product-1";

            // Act
            var result = await _productService.GetBySlugAsync(slug);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(slug, result.Slug);
        }

        [Fact]
        public async Task GetFeaturedProductsAsync_ReturnsFeaturedProducts()
        {
            // Act
            var result = await _productService.GetFeaturedProductsAsync(10);

            // Assert
            Assert.NotNull(result);
            var products = result.ToList();
            Assert.NotEmpty(products);
            Assert.All(products, p => Assert.True(p.IsFeatured));
        }

        [Fact]
        public async Task CreateAsync_ValidProduct_ReturnsCreatedProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Test Product",
                Description = "New product description",
                Price = 29.99m,
                StockQuantity = 50,
                CategoryId = 1,
                IsFeatured = false,
                ImageUrls = new List<string> { "https://example.com/image1.jpg" }
            };

            // Act
            var result = await _productService.CreateAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Price, result.Price);
            Assert.NotEmpty(result.Slug);
        }

        [Fact]
        public async Task UpdateAsync_ExistingProduct_ReturnsUpdatedProduct()
        {
            // Arrange
            int productId = 1;
            var updateDto = new UpdateProductDto
            {
                Name = "Updated Product Name",
                Price = 119.99m,
                StockQuantity = 15
            };

            // Act
            var result = await _productService.UpdateAsync(productId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Price, result.Price);
            Assert.Equal(updateDto.StockQuantity, result.StockQuantity);
        }

        [Fact]
        public async Task DeleteAsync_ExistingProduct_ReturnsTrue()
        {
            // Arrange
            int productId = 2;

            // Act
            var result = await _productService.DeleteAsync(productId);

            // Assert
            Assert.True(result);

            // Verify deletion
            var deletedProduct = await _productService.GetByIdAsync(productId);
            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task GetAllAsync_WithFilters_ReturnsFilteredProducts()
        {
            // Arrange
            var filter = new ProductFilterDto
            {
                Page = 1,
                PageSize = 10,
                MinPrice = 40m,
                MaxPrice = 100m,
                CategoryId = 1
            };

            // Act
            var result = await _productService.GetAllAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Items, p =>
            {
                Assert.True(p.Price >= filter.MinPrice);
                Assert.True(p.Price <= filter.MaxPrice);
                Assert.Equal(filter.CategoryId, p.CategoryId);
            });
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
