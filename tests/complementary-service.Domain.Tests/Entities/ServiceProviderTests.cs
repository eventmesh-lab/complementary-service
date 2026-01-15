using System;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.Entities
{
    public class ServiceProviderTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var name = "Test Provider";
            var serviceType = ServiceType.Catering;
            var endpoint = "http://api.test";
            var queue = "test-queue";
            var priority = 5;

            // Act
            var provider = new ServiceProvider(name, serviceType, endpoint, queue, priority);

            // Assert
            Assert.NotEqual(Guid.Empty, provider.Id);
            Assert.Equal(name, provider.Name);
            Assert.Equal(serviceType, provider.ServiceType);
            Assert.Equal(endpoint, provider.ApiEndpoint);
            Assert.Equal(queue, provider.QueueName);
            Assert.Equal(priority, provider.Priority);
            Assert.True(provider.IsActive);
        }

        [Fact]
        public void Deactivate_WhenCalled_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var provider = new ServiceProvider("Test", ServiceType.Catering, "ep", "q");

            // Act
            provider.Deactivate();

            // Assert
            Assert.False(provider.IsActive);
        }

        [Fact]
        public void Activate_WhenCalled_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var provider = new ServiceProvider("Test", ServiceType.Catering, "ep", "q");
            provider.Deactivate();

            // Act
            provider.Activate();

            // Assert
            Assert.True(provider.IsActive);
        }
    }
}
