using ComplementaryServices.Domain.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComplementaryServices.Application.Messaging.RabbitMQ
{
    public interface IServiceRequestPublisher
    {
        Task PublishServiceRequestAsync(
            Guid serviceId,
            Guid reservationId,
            Guid eventId,
            ServiceType serviceType,
            string details,
            CancellationToken cancellationToken = default);
    }
}
