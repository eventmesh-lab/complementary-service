// Domain/Events/ServiceConfirmedDomainEvent.cs
using System;
using ComplementaryServices.Domain.ValueObjects;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.Events
{
    public class ServiceConfirmedDomainEvent : IDomainEvent
    {
        public Guid ServiceId { get; }
        public Guid ReservationId { get; }
        public Guid UserId { get; }
        public ServiceType ServiceType { get; }
        public string ProviderId { get; }
        public decimal Price { get; }
        public DateTime OccurredOn { get; }

        public ServiceConfirmedDomainEvent(
            Guid serviceId,
            Guid reservationId,
            Guid userId,
            ServiceType serviceType,
            string providerId,
            decimal price)
        {
            ServiceId = serviceId;
            ReservationId = reservationId;
            UserId = userId;
            ServiceType = serviceType;
            ProviderId = providerId;
            Price = price;
            OccurredOn = DateTime.UtcNow;
        }
    }
}