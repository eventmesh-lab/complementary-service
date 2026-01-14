// Domain/Events/ServiceRejectedDomainEvent.cs
using System;
using ComplementaryServices.Domain.ValueObjects;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.Events
{
    public class ServiceRejectedDomainEvent : IDomainEvent
    {
        public Guid ServiceId { get; }
        public Guid ReservationId { get; }
        public Guid UserId { get; }
        public ServiceType ServiceType { get; }
        public string RejectionReason { get; }
        public DateTime OccurredOn { get; }

        public ServiceRejectedDomainEvent(
            Guid serviceId,
            Guid reservationId,
            Guid userId,
            ServiceType serviceType,
            string rejectionReason)
        {
            ServiceId = serviceId;
            ReservationId = reservationId;
            UserId = userId;
            ServiceType = serviceType;
            RejectionReason = rejectionReason;
            OccurredOn = DateTime.UtcNow;
        }
    }
}