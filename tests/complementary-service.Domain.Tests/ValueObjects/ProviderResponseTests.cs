using System;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.ValueObjects
{
    public class ProviderResponseTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
        {
            // Arrange
            var providerId = "p1";
            var message = "OK";
            var price = 50.5m;
            var estimatedTime = DateTime.UtcNow.AddHours(1);

            // Act
            var response = new ProviderResponse(true, providerId, message, price, estimatedTime);

            // Assert
            Assert.True(response.IsAvailable);
            Assert.Equal(providerId, response.ProviderId);
            Assert.Equal(message, response.Message);
            Assert.Equal(price, response.Price);
            Assert.Equal(estimatedTime, response.EstimatedTime);
        }

        [Fact]
        public void Constructor_WithNullProviderId_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ProviderResponse(true, null, "OK", 10m));
        }

        [Fact]
        public void Equality_SameValues_ShouldBeEqual()
        {
            // Arrange
            var r1 = new ProviderResponse(true, "p1", "m", 10m);
            var r2 = new ProviderResponse(true, "p1", "diff-message", 10m); // message not in equality components

            // Assert
            Assert.Equal(r1, r2);
        }
    }
}
