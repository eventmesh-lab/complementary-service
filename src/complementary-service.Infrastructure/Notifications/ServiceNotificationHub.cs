using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ComplementaryServices.Infrastructure.Notifications
{
    /// <summary>
    /// Hub de SignalR para notificaciones de servicios complementarios
    /// </summary>
    public class ServiceNotificationHub : Hub
    {
        private readonly ILogger<ServiceNotificationHub> _logger;

        public ServiceNotificationHub(ILogger<ServiceNotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("User {UserId} connected to ServiceNotificationHub", userId);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("User {UserId} disconnected from ServiceNotificationHub", userId);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Método que el cliente puede llamar para confirmar recepción de notificación
        /// </summary>
        public async Task AcknowledgeNotification(Guid serviceId)
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation(
                "User {UserId} acknowledged notification for service {ServiceId}",
                userId,
                serviceId);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Método para que el cliente solicite el estado actual de un servicio
        /// </summary>
        public async Task RequestServiceStatus(Guid serviceId)
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation(
                "User {UserId} requested status for service {ServiceId}",
                userId,
                serviceId);

            // Aquí podrías implementar lógica para obtener y enviar el estado actual
            await Clients.Caller.SendAsync("ServiceStatusResponse", new
            {
                ServiceId = serviceId,
                RequestedAt = DateTime.UtcNow
            });
        }
    }
}
