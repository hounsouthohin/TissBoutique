using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetUserCartAsync(string userId);
        Task<CartDto> AddItemAsync(string userId, AddToCartDto dto);
        Task<CartDto> UpdateItemQuantityAsync(string userId, int productId, int quantity);
        Task<CartDto> RemoveItemAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}
