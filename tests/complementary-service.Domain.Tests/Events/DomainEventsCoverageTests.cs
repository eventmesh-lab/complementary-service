using System;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.Events
{
    public class DomainEventsCoverageTests
    {
        [Fact]
        public void ServiceCancelledDomainEvent_ShouldSetProperties()
        {
            var id = Guid.NewGuid();
            var resId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var type = ServiceType.Transport;
            
            var evt = new ServiceCancelledDomainEvent(id, resId, userId, type);

            Assert.Equal(id, evt.ServiceId);
            Assert.Equal(resId, evt.ReservationId);
            Assert.Equal(userId, evt.UserId);
            Assert.Equal(type, evt.ServiceType);
            Assert.NotEqual(default(DateTime), evt.OccurredOn);
        }

        [Fact]
        public void ServiceConfirmedDomainEvent_ShouldSetProperties()
        {
             var id = Guid.NewGuid();
            var resId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var type = ServiceType.Transport;
            var pid = "p1";
            var price = 100m;

            var evt = new ServiceConfirmedDomainEvent(id, resId, userId, type, pid, price);

             Assert.Equal(id, evt.ServiceId);
            Assert.Equal(resId, evt.ReservationId);
            Assert.Equal(userId, evt.UserId);
            Assert.Equal(type, evt.ServiceType);
            Assert.Equal(pid, evt.ProviderId);
            Assert.Equal(price, evt.Price);
            Assert.NotEqual(default(DateTime), evt.OccurredOn);
        }

        [Fact]
        public void ServiceRejectedDomainEvent_ShouldSetProperties()
        {
            var id = Guid.NewGuid();
            var resId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var type = ServiceType.Transport;
            var reason = "reason";

            var evt = new ServiceRejectedDomainEvent(id, resId, userId, type, reason);

            Assert.Equal(id, evt.ServiceId);
            Assert.Equal(resId, evt.ReservationId);
            Assert.Equal(userId, evt.UserId);
            Assert.Equal(type, evt.ServiceType);
            Assert.Equal(reason, evt.RejectionReason);
            Assert.NotEqual(default(DateTime), evt.OccurredOn);
        }

        [Fact]
        public void ServiceRequestedDomainEvent_ShouldSetProperties()
        {
            var id = Guid.NewGuid();
            var resId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var type = ServiceType.Transport;
            var details = "details";

            var evt = new ServiceRequestedDomainEvent(id, resId, userId, eventId, type, details);

            Assert.Equal(id, evt.ServiceId);
            Assert.Equal(resId, evt.ReservationId);
            Assert.Equal(userId, evt.UserId);
            Assert.Equal(eventId, evt.EventId);
            Assert.Equal(type, evt.ServiceType);
            Assert.Equal(details, evt.Details);
            Assert.NotEqual(default(DateTime), evt.OccurredOn);
        }
    }
}
