using MediatR;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Infrastructure.Notifications;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.EventHandlers
{
    /// <summary>
    /// Maneja el evento cuando un servicio es rechazado
    /// Notifica al usuario mediante SignalR
    /// </summary>
    public class ServiceRejectedEventHandler : INotificationHandler<ServiceRejectedDomainEvent>
    {
        private readonly IServiceNotifier _notifier;
        private readonly ILogger<ServiceRejectedEventHandler> _logger;

        public ServiceRejectedEventHandler(
            IServiceNotifier notifier,
            ILogger<ServiceRejectedEventHandler> logger)
        {
            _notifier = notifier;
            _logger = logger;
        }

        public async Task Handle(ServiceRejectedDomainEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying user {UserId} about rejected service {ServiceId}",
                    notification.UserId,
                    notification.ServiceId);

                await _notifier.NotifyServiceRejectedAsync(
                    notification.UserId,
                    notification.ServiceId,
                    notification.ServiceType.Value,
                    notification.RejectionReason,
                    cancellationToken);

                _logger.LogInformation(
                    "User {UserId} notified about rejection of service {ServiceId}",
                    notification.UserId,
                    notification.ServiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error notifying user {UserId} about rejected service {ServiceId}",
                    notification.UserId,
                    notification.ServiceId);
            }
        }
    }
}
