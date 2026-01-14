using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.Notifications;
using ComplementaryServices.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.EventHandlers
{
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
            _logger.LogInformation("Handling ServiceConfirmedDomainEvent for Service {ServiceId}", notification.ServiceId);

            await _notifier.NotifyServiceConfirmedAsync(
                notification.UserId,
                notification.ServiceId,
                notification.ServiceType.Value,
                notification.ProviderId,
                notification.Price,
                cancellationToken);
        }
    }
}
