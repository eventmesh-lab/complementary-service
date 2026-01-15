using System;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.ValueObjects
{
    public class ValueObjectsCoverageTests
    {
        [Fact]
        public void ServiceStatus_FromString_ShouldHandleVariousInputs()
        {
            Assert.Equal(ServiceStatus.Requested, ServiceStatus.FromString("Requested"));
            Assert.Equal(ServiceStatus.Pending, ServiceStatus.FromString("Pending"));
            Assert.Equal(ServiceStatus.Confirmed, ServiceStatus.FromString("Confirmed"));
            Assert.Equal(ServiceStatus.Rejected, ServiceStatus.FromString("Rejected"));
            Assert.Equal(ServiceStatus.Cancelled, ServiceStatus.FromString("Cancelled"));

            Assert.Throws<ArgumentException>(() => ServiceStatus.FromString("Invalid"));
            Assert.Throws<ArgumentException>(() => ServiceStatus.FromString(null));
        }

        [Fact] 
        public void ServiceType_FromString_ShouldHandleVariousInputs()
        {
            Assert.Equal(ServiceType.Transport, ServiceType.FromString("Transport"));
            Assert.Equal(ServiceType.Catering, ServiceType.FromString("Catering"));
            
            Assert.Throws<ArgumentException>(() => ServiceType.FromString("Invalid"));
            Assert.Throws<ArgumentException>(() => ServiceType.FromString(null));
        }

        [Fact]
        public void ServiceProvider_Accessors_ShouldWork()
        {
            var p = new ServiceProvider("Name", ServiceType.Catering, "http://url", "queue", 1);
            p.Activate();
            Assert.True(p.IsActive);
            p.Deactivate();
            Assert.False(p.IsActive);

            Assert.Equal("Name", p.Name);
            Assert.Equal(ServiceType.Catering, p.ServiceType);
            // etc... this is covered by ServiceProviderTests theoretically but adding simple checks here failsafe
        }

        [Fact]
        public void ProviderResponse_GetEqualityComponents_Coverage()
        {
            var p1 = new ProviderResponse(true, "p", "m", 1m);
            var p2 = new ProviderResponse(true, "p", "m", 1m);
            Assert.Equal(p1, p2);
            Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
        }

        [Fact]
        public void ServiceType_Equality_Coverage()
        {
             var t1 = ServiceType.Catering;
             var t2 = ServiceType.FromString("Catering");
             Assert.Equal(t1, t2);
             Assert.Equal(t1.GetHashCode(), t2.GetHashCode());
        }
    }
}
