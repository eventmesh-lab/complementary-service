using System;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.ValueObjects
{
    public class ServiceTypeTests
    {
        [Theory]
        [InlineData("Transport", "Transport")]
        [InlineData("Catering", "Catering")]
        [InlineData("Merchandising", "Merchandising")]
        [InlineData("transport", "Transport")]
        public void FromString_WithValidValue_ShouldReturnCorrectType(string input, string expectedValue)
        {
            // Act
            var result = ServiceType.FromString(input);

            // Assert
            Assert.Equal(expectedValue, result.Value);
        }

        [Fact]
        public void FromString_WithInvalidValue_ShouldThrowArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ServiceType.FromString("Invalid"));
        }

        [Fact]
        public void Equality_SameValues_ShouldBeEqual()
        {
            // Arrange
            var type1 = ServiceType.FromString("Catering");
            var type2 = ServiceType.FromString("catering");

            // Assert
            Assert.Equal(type1, type2);
        }
    }
}
