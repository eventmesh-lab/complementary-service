using MediatR;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Infrastructure.Messaging.RabbitMQ;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.EventHandlers
{
    /// <summary>
    /// Maneja el evento de dominio cuando se solicita un servicio
    /// Publica mensaje a RabbitMQ para integración con proveedores
    /// </summary>
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
            try
            {
                _logger.LogInformation(
                    "Publishing service request {ServiceId} to RabbitMQ for {ServiceType}",
                    notification.ServiceId,
                    notification.ServiceType.Value);

                await _publisher.PublishServiceRequestAsync(
                    notification.ServiceId,
                    notification.ReservationId,
                    notification.EventId,
                    notification.ServiceType,
                    notification.Details,
                    cancellationToken);

                _logger.LogInformation(
                    "Service request {ServiceId} published successfully",
                    notification.ServiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error publishing service request {ServiceId} to RabbitMQ",
                    notification.ServiceId);
                throw;
            }
        }
    }
}
