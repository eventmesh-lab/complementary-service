using System;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.ValueObjects
{
    public class ServiceStatusFromStringTests
    {
        [Theory]
        [InlineData("Requested", "Requested")]
        [InlineData("requested", "Requested")]
        [InlineData("Pending", "Pending")]
        [InlineData("pending", "Pending")]
        [InlineData("Confirmed", "Confirmed")]
        [InlineData("confirmed", "Confirmed")]
        [InlineData("Rejected", "Rejected")]
        [InlineData("rejected", "Rejected")]
        [InlineData("Cancelled", "Cancelled")]
        [InlineData("cancelled", "Cancelled")]
        public void FromString_WithValidValues_ShouldReturnExpected(string input, string expected)
        {
            var result = ServiceStatus.FromString(input);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void FromString_WithInvalidValue_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ServiceStatus.FromString("unknown-status"));
        }

        [Fact]
        public void FromString_WithNull_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ServiceStatus.FromString(null));
        }
    }
}
