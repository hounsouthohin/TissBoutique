using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetUserCartAsync(string userId);
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task ClearCartAsync(int cartId);
    }
}
