using MediatR;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.EventHandlers
{
    /// <summary>
    /// Maneja el evento cuando un servicio es confirmado
    /// Envía notificación en tiempo real al usuario vía SignalR
    /// </summary>
    public class ServiceConfirmedEventHandler : INotificationHandler<ServiceConfirmedDomainEvent>
    {
        private readonly IServiceNotifier _notifier;
        private readonly ILogger<ServiceConfirmedEventHandler> _logger;

        public ServiceConfirmedEventHandler(
            IServiceNotifier notifier,
            ILogger<ServiceConfirmedEventHandler> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Handle(ServiceConfirmedDomainEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying user {UserId} about confirmed service {ServiceId}",
                    notification.UserId,
                    notification.ServiceId);

                await _notifier.NotifyServiceConfirmedAsync(
                    notification.UserId,
                    notification.ServiceId,
                    notification.ServiceType.Value,
                    notification.ProviderId,
                    notification.Price,
                    cancellationToken);

                _logger.LogInformation(
                    "User {UserId} notified successfully about service {ServiceId}",
                    notification.UserId,
                    notification.ServiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error notifying user {UserId} about service {ServiceId}",
                    notification.UserId,
                    notification.ServiceId);
                // No lanzar excepción para no afectar el flujo principal
            }
        }
    }
}
