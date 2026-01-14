// Domain/Events/ServiceCancelledDomainEvent.cs
using System;
using ComplementaryServices.Domain.ValueObjects;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.Events
{
    public class ServiceCancelledDomainEvent : IDomainEvent
    {
        public Guid ServiceId { get; }
        public Guid ReservationId { get; }
        public Guid UserId { get; }
        public ServiceType ServiceType { get; }
        public DateTime OccurredOn { get; }

        public ServiceCancelledDomainEvent(
            Guid serviceId,
            Guid reservationId,
            Guid userId,
            ServiceType serviceType)
        {
            ServiceId = serviceId;
            ReservationId = reservationId;
            UserId = userId;
            ServiceType = serviceType;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
