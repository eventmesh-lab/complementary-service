using System;
using ComplementaryServices.Domain.ValueObjects;
using Xunit;

namespace ComplementaryServices.Domain.Tests.ValueObjects
{
    public class ServiceTypeAdditionalTests
    {
        [Theory]
        [InlineData("Transport", "Transport")]
        [InlineData("transport", "Transport")]
        [InlineData("Catering", "Catering")]
        [InlineData("catering", "Catering")]
        [InlineData("Merchandising", "Merchandising")]
        [InlineData("merchandising", "Merchandising")]
        public void FromString_VariousCasing_ReturnsExpected(string input, string expected)
        {
            var result = ServiceType.FromString(input);
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public void FromString_Invalid_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ServiceType.FromString("invalid-type"));
        }

        [Fact]
        public void FromString_Null_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ServiceType.FromString(null));
        }
    }
}
