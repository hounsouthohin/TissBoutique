namespace ECommerce.Application.Interfaces
{
    public interface IAppHubClient
    {
        Task ReceiveOrderStatusUpdate(int orderId, string newStatus, string orderNumber);
        Task ReceiveGenericNotification(string message);
    }
}
