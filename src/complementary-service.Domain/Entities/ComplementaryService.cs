using System;
using ComplementaryServices.Domain.ValueObjects;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.Entities
{
    public class ComplementaryService : Entity, IAggregateRoot
    {
        public Guid ReservationId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid EventId { get; private set; }
        public ServiceType ServiceType { get; private set; }
        public ServiceStatus Status { get; private set; }
        public string ProviderId { get; private set; }
        public decimal Price { get; private set; }
        public string Details { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? RejectedAt { get; private set; }
        public string RejectionReason { get; private set; }
        public ProviderResponse ProviderResponse { get; private set; }

        private ComplementaryService() { } // EF Core

        public ComplementaryService(
            Guid reservationId,
            Guid userId,
            Guid eventId,
            ServiceType serviceType,
            string details)
        {
            Id = Guid.NewGuid();
            ReservationId = reservationId;
            UserId = userId;
            EventId = eventId;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Details = details;
            Status = ServiceStatus.Requested;
            RequestedAt = DateTime.UtcNow;

            AddDomainEvent(new ServiceRequestedDomainEvent(
                Id,
                ReservationId,
                UserId,
                EventId,
                ServiceType,
                Details));
        }

        public void MarkAsPending(string providerId)
        {
            if (Status != ServiceStatus.Requested)
                throw new InvalidOperationException("Only requested services can be marked as pending");

            Status = ServiceStatus.Pending;
            ProviderId = providerId;
        }

        public void Confirm(ProviderResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (!response.IsAvailable)
                throw new InvalidOperationException("Cannot confirm unavailable service");

            if (Status != ServiceStatus.Pending && Status != ServiceStatus.Requested)
                throw new InvalidOperationException($"Cannot confirm service in status {Status.Value}");

            Status = ServiceStatus.Confirmed;
            ConfirmedAt = DateTime.UtcNow;
            ProviderResponse = response;
            Price = response.Price;
            ProviderId = response.ProviderId;

            AddDomainEvent(new ServiceConfirmedDomainEvent(
                Id,
                ReservationId,
                UserId,
                ServiceType,
                ProviderId,
                Price));
        }

        public void Reject(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required", nameof(reason));

            if (Status == ServiceStatus.Confirmed)
                throw new InvalidOperationException("Cannot reject confirmed service");

            Status = ServiceStatus.Rejected;
            RejectedAt = DateTime.UtcNow;
            RejectionReason = reason;

            AddDomainEvent(new ServiceRejectedDomainEvent(
                Id,
                ReservationId,
                UserId,
                ServiceType,
                reason));
        }

        public void Cancel()
        {
            if (Status == ServiceStatus.Cancelled)
                throw new InvalidOperationException("Service already cancelled");

            Status = ServiceStatus.Cancelled;
        }
    }
}