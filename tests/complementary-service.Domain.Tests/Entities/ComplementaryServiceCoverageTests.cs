using System;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.Entities
{
    public class ComplementaryServiceCoverageTests
    {
        [Fact]
        public void MarkAsPending_ShouldChangeStatus_WhenRequested()
        {
            var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Transport, "details");
            service.MarkAsPending("provider1");
            Assert.Equal(ServiceStatus.Pending, service.Status);
            Assert.Equal("provider1", service.ProviderId);
        }

        [Fact]
        public void MarkAsPending_ShouldThrow_WhenNotRequested()
        {
            var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Transport, "details");
            service.Cancel();
            Assert.Throws<InvalidOperationException>(() => service.MarkAsPending("p1"));
        }

        [Fact]
        public void Confirm_ShouldThrow_WhenStatusIsInvalid()
        {
             var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Transport, "details");
             service.Cancel();
             var resp = new ProviderResponse(true, "p1", "ok", 10m);
             Assert.Throws<InvalidOperationException>(() => service.Confirm(resp));
        }

        [Fact] // Private ctor for EF coverage (reflection)
        public void PrivateConstructor_ShouldBeCallable()
        {
            var ctor = typeof(ComplementaryService).GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.NotNull(ctor);
            var instance = ctor.Invoke(null);
            Assert.NotNull(instance);
        }
    }
}
