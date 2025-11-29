using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendOrderStatusUpdateAsync(string userId, int orderId, string newStatus, string orderNumber);
        Task SendGenericNotificationToUserAsync(string userId, string message);
    }
}
