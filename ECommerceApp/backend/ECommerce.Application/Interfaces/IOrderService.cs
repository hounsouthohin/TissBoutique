using ECommerce.Application.DTOs.Orders;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto?> GetByOrderNumberAsync(string orderNumber);
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto dto);

        Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status);

        Task<OrderDto?> UpdateOrderStatusAsync(int id, UpdateOrderStatusDto dto);
        Task<OrderDto?> CancelOrderAsync(int id, string userId);
    }
}
