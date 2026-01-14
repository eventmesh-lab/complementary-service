using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.Notifications;

namespace ComplementaryServices.Infrastructure.Notifications
{
    /// <summary>
    /// TC-062: Implementación de notificaciones en tiempo real con SignalR
    /// </summary>
    public class SignalRServiceNotifier : IServiceNotifier
    {
        private readonly IHubContext<ServiceNotificationHub> _hubContext;
        private readonly ILogger<SignalRServiceNotifier> _logger;

        public SignalRServiceNotifier(
            IHubContext<ServiceNotificationHub> hubContext,
            ILogger<SignalRServiceNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyServiceConfirmedAsync(
            Guid userId,
            Guid serviceId,
            string serviceType,
            string providerId,
            decimal price,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new
                {
                    Type = "ServiceConfirmed",
                    ServiceId = serviceId,
                    ServiceType = serviceType,
                    ProviderId = providerId,
                    Price = price,
                    Message = $"Your {serviceType} service has been confirmed!",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ServiceNotification", notification, cancellationToken);

                _logger.LogInformation(
                    "Sent confirmation notification to user {UserId} for service {ServiceId}",
                    userId,
                    serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending confirmation notification to user {UserId} for service {ServiceId}",
                    userId,
                    serviceId);
                throw;
            }
        }

        public async Task NotifyServiceRejectedAsync(
            Guid userId,
            Guid serviceId,
            string serviceType,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new
                {
                    Type = "ServiceRejected",
                    ServiceId = serviceId,
                    ServiceType = serviceType,
                    Reason = reason,
                    Message = $"Unfortunately, your {serviceType} service request was not available.",
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ServiceNotification", notification, cancellationToken);

                _logger.LogInformation(
                    "Sent rejection notification to user {UserId} for service {ServiceId}",
                    userId,
                    serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending rejection notification to user {UserId} for service {ServiceId}",
                    userId,
                    serviceId);
                throw;
            }
        }

        public async Task NotifyServiceUpdatedAsync(
            Guid userId,
            Guid serviceId,
            string status,
            string message,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new
                {
                    Type = "ServiceUpdated",
                    ServiceId = serviceId,
                    Status = status,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ServiceNotification", notification, cancellationToken);

                _logger.LogInformation(
                    "Sent update notification to user {UserId} for service {ServiceId}",
                    userId,
                    serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending update notification to user {UserId} for service {ServiceId}",
                    userId,
                    serviceId);
                throw;
            }
        }
    }
}
