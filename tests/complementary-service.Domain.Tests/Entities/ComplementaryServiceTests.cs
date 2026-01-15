using System;
using System.Linq;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.Entities
{
    public class ComplementaryServiceTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var reservationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var serviceType = ServiceType.Catering;
            var details = "Service details";

            // Act
            var service = new ComplementaryService(reservationId, userId, eventId, serviceType, details);

            // Assert
            Assert.NotEqual(Guid.Empty, service.Id);
            Assert.Equal(reservationId, service.ReservationId);
            Assert.Equal(userId, service.UserId);
            Assert.Equal(eventId, service.EventId);
            Assert.Equal(serviceType, service.ServiceType);
            Assert.Equal(details, service.Details);
            Assert.Equal(ServiceStatus.Requested, service.Status);
            Assert.True(service.RequestedAt <= DateTime.UtcNow);
            Assert.Single(service.DomainEvents);
            Assert.IsType<ServiceRequestedDomainEvent>(service.DomainEvents.First());
        }

        [Fact]
        public void Confirm_WhenStatusIsPending_ShouldUpdateToConfirmed()
        {
            // Arrange
            var service = CreateRequestedService();
            service.MarkAsPending("provider-123");
            var providerResponse = new ProviderResponse(true, "provider-123", "OK", 100.0m);

            // Act
            service.Confirm(providerResponse);

            // Assert
            Assert.Equal(ServiceStatus.Confirmed, service.Status);
            Assert.Equal(providerResponse.Price, service.Price);
            Assert.Equal(providerResponse.ProviderId, service.ProviderId);
            Assert.NotNull(service.ConfirmedAt);
            Assert.Contains(service.DomainEvents, e => e is ServiceConfirmedDomainEvent);
        }

        [Fact]
        public void Confirm_WhenStatusIsRequested_ShouldUpdateToConfirmed()
        {
            // Arrange
            var service = CreateRequestedService();
            var providerResponse = new ProviderResponse(true, "provider-123", "OK", 100.0m);

            // Act
            service.Confirm(providerResponse);

            // Assert
            Assert.Equal(ServiceStatus.Confirmed, service.Status);
        }

        [Fact]
        public void Confirm_WhenResponseIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var service = CreateRequestedService();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => service.Confirm(null));
        }

        [Fact]
        public void Confirm_WhenServiceUnavailable_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateRequestedService();
            var providerResponse = new ProviderResponse(false, "provider-123", "Not available", 0m);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.Confirm(providerResponse));
        }

        [Fact]
        public void Reject_WhenStatusIsNotConfirmed_ShouldUpdateToRejected()
        {
            // Arrange
            var service = CreateRequestedService();
            var reason = "Out of stock";

            // Act
            service.Reject(reason);

            // Assert
            Assert.Equal(ServiceStatus.Rejected, service.Status);
            Assert.Equal(reason, service.RejectionReason);
            Assert.NotNull(service.RejectedAt);
            Assert.Contains(service.DomainEvents, e => e is ServiceRejectedDomainEvent);
        }

        [Fact]
        public void Reject_WhenReasonIsMissing_ShouldThrowArgumentException()
        {
            // Arrange
            var service = CreateRequestedService();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => service.Reject(null));
            Assert.Throws<ArgumentException>(() => service.Reject(""));
        }

        [Fact]
        public void Reject_WhenAlreadyConfirmed_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateRequestedService();
            service.Confirm(new ProviderResponse(true, "p1", "OK", 10m));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.Reject("Any reason"));
        }

        [Fact]
        public void Cancel_WhenNotAlreadyCancelled_ShouldSetStatusToCancelled()
        {
            // Arrange
            var service = CreateRequestedService();

            // Act
            service.Cancel();

            // Assert
            Assert.Equal(ServiceStatus.Cancelled, service.Status);
        }

        [Fact]
        public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateRequestedService();
            service.Cancel();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.Cancel());
        }

        [Fact]
        public void MarkAsPending_WhenRequested_ShouldUpdateStatus()
        {
            // Arrange
            var service = CreateRequestedService();

            // Act
            service.MarkAsPending("provider-1");

            // Assert
            Assert.Equal(ServiceStatus.Pending, service.Status);
            Assert.Equal("provider-1", service.ProviderId);
        }

        [Fact]
        public void MarkAsPending_WhenNotRequested_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var service = CreateRequestedService();
            service.Cancel();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.MarkAsPending("p1"));
        }

        private ComplementaryService CreateRequestedService()
        {
            return new ComplementaryService(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ServiceType.Catering,
                "Test details");
        }
    }
}
