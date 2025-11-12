using ECommerce.API.Controllers;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.Tests.Integration
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetById_ExistingProduct_ReturnsOkWithProduct()
        {
            // Arrange
            int productId = 1;
            var expectedProduct = new ProductDto
            {
                Id = productId,
                Name = "Test Product",
                Slug = "test-product",
                Price = 99.99m,
                StockQuantity = 10
            };

            _productServiceMock
                .Setup(s => s.GetByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(productId, returnedProduct.Id);
            Assert.Equal(expectedProduct.Name, returnedProduct.Name);
        }

        [Fact]
        public async Task GetById_NonExistingProduct_ReturnsNotFound()
        {
            // Arrange
            int nonExistingId = 999;
            _productServiceMock
                .Setup(s => s.GetByIdAsync(nonExistingId))
                .ReturnsAsync((ProductDto?)null);

            // Act
            var result = await _controller.GetById(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetBySlug_ExistingSlug_ReturnsOkWithProduct()
        {
            // Arrange
            string slug = "test-product";
            var expectedProduct = new ProductDto
            {
                Id = 1,
                Name = "Test Product",
                Slug = slug,
                Price = 99.99m
            };

            _productServiceMock
                .Setup(s => s.GetBySlugAsync(slug))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetBySlug(slug);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(slug, returnedProduct.Slug);
        }

        [Fact]
        public async Task GetFeatured_ReturnsOkWithProducts()
        {
            // Arrange
            var featuredProducts = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Featured 1", IsFeatured = true },
                new ProductDto { Id = 2, Name = "Featured 2", IsFeatured = true }
            };

            _productServiceMock
                .Setup(s => s.GetFeaturedProductsAsync(10))
                .ReturnsAsync(featuredProducts);

            // Act
            var result = await _controller.GetFeatured(10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
            Assert.Equal(2, returnedProducts.Count());
        }

        [Fact]
        public async Task Create_ValidProduct_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "Description",
                Price = 49.99m,
                StockQuantity = 20,
                CategoryId = 1
            };

            var createdProduct = new ProductDto
            {
                Id = 3,
                Name = createDto.Name,
                Slug = "new-product",
                Price = createDto.Price
            };

            _productServiceMock
                .Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(createdResult.Value);
            Assert.Equal(createdProduct.Id, returnedProduct.Id);
        }

        [Fact]
        public async Task Update_ExistingProduct_ReturnsOkWithUpdatedProduct()
        {
            // Arrange
            int productId = 1;
            var updateDto = new UpdateProductDto
            {
                Name = "Updated Product",
                Price = 79.99m
            };

            var updatedProduct = new ProductDto
            {
                Id = productId,
                Name = updateDto.Name,
                Price = updateDto.Price.Value
            };

            _productServiceMock
                .Setup(s => s.UpdateAsync(productId, updateDto))
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.Update(productId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
            Assert.Equal(updateDto.Name, returnedProduct.Name);
        }

        [Fact]
        public async Task Delete_ExistingProduct_ReturnsNoContent()
        {
            // Arrange
            int productId = 1;
            _productServiceMock
                .Setup(s => s.DeleteAsync(productId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(productId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_NonExistingProduct_ReturnsNotFound()
        {
            // Arrange
            int nonExistingId = 999;
            _productServiceMock
                .Setup(s => s.DeleteAsync(nonExistingId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(nonExistingId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
