using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.Messaging.RabbitMQ;
using ComplementaryServices.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.EventHandlers
{
    public class ServiceRequestedEventHandler : INotificationHandler<ServiceRequestedDomainEvent>
    {
        private readonly IServiceRequestPublisher _publisher;
        private readonly ILogger<ServiceRequestedEventHandler> _logger;

        public ServiceRequestedEventHandler(
            IServiceRequestPublisher publisher,
            ILogger<ServiceRequestedEventHandler> logger)
        {
            _publisher = publisher;
            _logger = logger;
        }

        public async Task Handle(ServiceRequestedDomainEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling ServiceRequestedDomainEvent for Service {ServiceId}", notification.ServiceId);

            await _publisher.PublishServiceRequestAsync(
                notification.ServiceId,
                notification.ReservationId,
                notification.EventId,
                notification.ServiceType,
                notification.Details,
                cancellationToken);
        }
    }
}
