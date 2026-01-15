using System;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.ValueObjects
{
    public class ServiceStatusTests
    {
        [Theory]
        [InlineData("Requested", "Requested")]
        [InlineData("Pending", "Pending")]
        [InlineData("Confirmed", "Confirmed")]
        [InlineData("Rejected", "Rejected")]
        [InlineData("Cancelled", "Cancelled")]
        public void FromString_WithValidValue_ShouldReturnCorrectStatus(string input, string expectedValue)
        {
            // Act
            var result = ServiceStatus.FromString(input);

            // Assert
            Assert.Equal(expectedValue, result.Value);
        }

        [Fact]
        public void FromString_WithInvalidValue_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ServiceStatus.FromString("Unknown"));
        }
    }
}
