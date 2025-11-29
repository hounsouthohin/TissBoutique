using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ECommerce.API.Hubs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, IAppHubClient> _hubContext;

        public SignalRNotificationService(IHubContext<NotificationHub, IAppHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendOrderStatusUpdateAsync(string userId, int orderId, string newStatus, string orderNumber)
        {
            await _hubContext.Clients.Group(userId).ReceiveOrderStatusUpdate(orderId, newStatus, orderNumber);
        }

        public async Task SendGenericNotificationToUserAsync(string userId, string message)
        {
            await _hubContext.Clients.Group(userId).ReceiveGenericNotification(message);
        }
    }
}
