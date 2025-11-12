using AutoMapper;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mappings;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit
{
    public class OrderServiceTests : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly OrderService _orderService;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        public OrderServiceTests()
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

            // Mock EmailService
            _emailServiceMock = new Mock<IEmailService>();

            // ✅ Mock Logger<OrderService>
            _loggerMock = new Mock<ILogger<OrderService>>();

            // ✅ Create OrderService instance
            _orderService = new OrderService(
                _unitOfWork,
                _mapper,
                _emailServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetUserOrdersAsync_ExistingUser_ReturnsOrders()
        {
            // Arrange
            string userId = "test-user-id";

            // Create a test order
            var order = new Order
            {
                OrderNumber = "TEST-001",
                UserId = userId,
                Status = OrderStatus.Pending,
                SubtotalAmount = 100m,
                TaxAmount = 15m,
                ShippingAmount = 10m,
                TotalAmount = 125m,
                ShippingStreet = "123 Test St",
                ShippingCity = "Test City",
                ShippingProvince = "QC",
                ShippingPostalCode = "H1H 1H1",
                ShippingCountry = "Canada",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _orderService.GetUserOrdersAsync(userId);

            // Assert
            Assert.NotNull(result);
            var orders = result.ToList();
            Assert.NotEmpty(orders);
            Assert.All(orders, o => Assert.Equal(userId, o.UserId));
        }

        [Fact]
        public async Task GetByOrderNumberAsync_ExistingOrder_ReturnsOrder()
        {
            // Arrange
            string orderNumber = "TEST-002";
            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = "test-user-id",
                Status = OrderStatus.Pending,
                SubtotalAmount = 50m,
                TaxAmount = 7.5m,
                ShippingAmount = 5m,
                TotalAmount = 62.5m,
                ShippingStreet = "456 Test Ave",
                ShippingCity = "Test City",
                ShippingProvince = "QC",
                ShippingPostalCode = "H2H 2H2",
                ShippingCountry = "Canada",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _orderService.GetByOrderNumberAsync(orderNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderNumber, result.OrderNumber);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ValidStatusChange_UpdatesSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                OrderNumber = "TEST-003",
                UserId = "test-user-id",
                Status = OrderStatus.Pending,
                SubtotalAmount = 75m,
                TaxAmount = 11.25m,
                ShippingAmount = 8m,
                TotalAmount = 94.25m,
                ShippingStreet = "789 Test Blvd",
                ShippingCity = "Test City",
                ShippingProvince = "QC",
                ShippingPostalCode = "H3H 3H3",
                ShippingCountry = "Canada",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var updateDto = new UpdateOrderStatusDto
            {
                Status = OrderStatus.Shipped,
                TrackingNumber = "TRACK123456"
            };

            // Act
            var result = await _orderService.UpdateOrderStatusAsync(order.Id, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderStatus.Shipped, result.Status);
            Assert.Equal(updateDto.TrackingNumber, result.TrackingNumber);
            Assert.NotNull(result.ShippedAt);
        }

        [Fact]
        public async Task CancelOrderAsync_PendingOrder_CancelsSuccessfully()
        {
            // Arrange
            var product = await _unitOfWork.Products.GetByIdAsync(1);
            Assert.NotNull(product);

            var initialStock = product.StockQuantity;

            var order = new Order
            {
                OrderNumber = "TEST-004",
                UserId = "test-user-id",
                Status = OrderStatus.Pending,
                SubtotalAmount = 99.99m,
                TaxAmount = 15m,
                ShippingAmount = 10m,
                TotalAmount = 124.99m,
                ShippingStreet = "321 Test Rd",
                ShippingCity = "Test City",
                ShippingProvince = "QC",
                ShippingPostalCode = "H4H 4H4",
                ShippingCountry = "Canada",
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 2,
                        UnitPrice = product.Price,
                        Discount = 0
                    }
                }
            };

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _orderService.CancelOrderAsync(order.Id, "test-user-id");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(OrderStatus.Cancelled, result.Status);

            // Verify stock was restored
            var updatedProduct = await _unitOfWork.Products.GetByIdAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(initialStock + 2, updatedProduct.StockQuantity);
        }

        public void Dispose()
        {
            // Cleanup
        }
    }
}
