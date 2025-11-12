using AutoMapper;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CartDto> GetUserCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> AddItemAsync(string userId, AddToCartDto dto)
        {
            var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new NotFoundException("Product not found");

            if (product.StockQuantity < dto.Quantity)
                throw new BadRequestException("Insufficient stock");

            var existingItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                if (existingItem.Quantity > product.StockQuantity)
                    throw new BadRequestException("Insufficient stock");
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.Price,
                    AddedAt = DateTime.UtcNow
                };
                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> UpdateItemQuantityAsync(string userId, int productId, int quantity)
        {
            var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cart == null)
                throw new NotFoundException("Cart not found");

            var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cart.Id, productId);
            if (cartItem == null)
                throw new NotFoundException("Cart item not found");

            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException("Product not found");

            if (quantity > product.StockQuantity)
                throw new BadRequestException("Insufficient stock");

            cartItem.Quantity = quantity;
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto> RemoveItemAsync(string userId, int productId)
        {
            var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cart == null)
                throw new NotFoundException("Cart not found");

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null)
                throw new NotFoundException("Cart item not found");

            cart.Items.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cart);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetUserCartAsync(userId);
            if (cart != null)
            {
                await _unitOfWork.Carts.ClearCartAsync(cart.Id);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
