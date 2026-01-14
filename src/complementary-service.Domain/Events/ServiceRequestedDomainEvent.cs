// Domain/Events/ServiceRequestedDomainEvent.cs
using System;
using ComplementaryServices.Domain.ValueObjects;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.Events
{
    public class ServiceRequestedDomainEvent : IDomainEvent
    {
        public Guid ServiceId { get; }
        public Guid ReservationId { get; }
        public Guid UserId { get; }
        public Guid EventId { get; }
        public ServiceType ServiceType { get; }
        public string Details { get; }
        public DateTime OccurredOn { get; }

        public ServiceRequestedDomainEvent(
            Guid serviceId,
            Guid reservationId,
            Guid userId,
            Guid eventId,
            ServiceType serviceType,
            string details)
        {
            ServiceId = serviceId;
            ReservationId = reservationId;
            UserId = userId;
            EventId = eventId;
            ServiceType = serviceType;
            Details = details;
            OccurredOn = DateTime.UtcNow;
        }
    }
}