using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.Notifications;
using ComplementaryServices.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.EventHandlers
{
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
            _logger.LogInformation("Handling ServiceRejectedDomainEvent for Service {ServiceId}", notification.ServiceId);

            await _notifier.NotifyServiceRejectedAsync(
                notification.UserId,
                notification.ServiceId,
                notification.ServiceType.Value,
                notification.RejectionReason,
                cancellationToken);
        }
    }
}
