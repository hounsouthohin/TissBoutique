using AutoMapper;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEmailService emailService,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _unitOfWork.Orders.GetUserOrdersAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
        {
            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto)
        {
            var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cart == null || !cart.Items.Any())
                throw new BadRequestException("Cart is empty");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orderNumber = await _unitOfWork.Orders.GenerateOrderNumberAsync();

                var order = new Order
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    Status = OrderStatus.Pending,
                    ShippingStreet = dto.ShippingAddress.Street,
                    ShippingCity = dto.ShippingAddress.City,
                    ShippingProvince = dto.ShippingAddress.Province,
                    ShippingPostalCode = dto.ShippingAddress.PostalCode,
                    ShippingCountry = dto.ShippingAddress.Country,
                    ShippingAmount = 10.00m,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                foreach (var cartItem in cart.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                    if (product == null)
                        throw new NotFoundException($"Product {cartItem.ProductId} not found");

                    if (product.StockQuantity < cartItem.Quantity)
                        throw new BadRequestException($"Insufficient stock for {product.Name}");

                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        Discount = 0
                    };

                    order.Items.Add(orderItem);

                    product.StockQuantity -= cartItem.Quantity;
                    _unitOfWork.Products.Update(product);
                }

                order.CalculateTotals();

                var payment = new Payment
                {
                    StripePaymentIntentId = dto.PaymentIntentId,
                    Amount = order.TotalAmount,
                    Currency = "CAD",
                    Status = PaymentStatus.Completed,
                    PaymentMethod = "card",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                };

                order.Payment = payment;

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.Carts.ClearCartAsync(cart.Id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Order {OrderNumber} created successfully for user {UserId}",
                    orderNumber, userId);

                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // ✅ MÉTHODE AVEC DTO (pour les appels API normaux)
        public async Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return null;

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == OrderStatus.Shipped && !string.IsNullOrWhiteSpace(dto.TrackingNumber))
            {
                order.TrackingNumber = dto.TrackingNumber;
                order.ShippedAt = DateTime.UtcNow;
            }

            if (dto.Status == OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            if (dto.Status == OrderStatus.Cancelled)
            {
                order.CancelledAt = DateTime.UtcNow;
            }

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(); // ✅ UTILISE SaveChangesAsync

            _logger.LogInformation("Order {OrderId} status updated to {Status}", id, dto.Status);

            return _mapper.Map<OrderDto>(order);
        }

        // ✅ MÉTHODE AVEC ENUM (pour les webhooks)
        public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order {orderId} not found");
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            // Mettre à jour les timestamps selon le statut
            switch (status)
            {
                case OrderStatus.Processing:
                    _logger.LogInformation("Order {OrderId} is now being processed", orderId);
                    break;

                case OrderStatus.Shipped:
                    order.ShippedAt = DateTime.UtcNow;
                    _logger.LogInformation("Order {OrderId} has been shipped", orderId);
                    break;

                case OrderStatus.Delivered:
                    order.DeliveredAt = DateTime.UtcNow;
                    _logger.LogInformation("Order {OrderId} has been delivered", orderId);
                    break;

                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    _logger.LogWarning("Order {OrderId} has been cancelled", orderId);
                    break;

                case OrderStatus.Refunded:
                    _logger.LogInformation("Order {OrderId} has been refunded", orderId);
                    break;
            }

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(); // ✅ CORRIGÉ : SaveChangesAsync au lieu de CompleteAsync

            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto?> CancelOrderAsync(int id, string userId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
                return null;

            if (order.UserId != userId)
                throw new UnauthorizedAccessException("Not authorized");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
                throw new BadRequestException("Order cannot be cancelled");

            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Remettre les produits en stock
            foreach (var item in order.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _unitOfWork.Products.Update(product);
                }
            }

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", id, userId);

            return _mapper.Map<OrderDto>(order);
        }
    }
}