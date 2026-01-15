using System;
using ComplementaryServices.Domain.Entities;
using Xunit;

namespace ComplementaryServices.Domain.Tests.Entities
{
    public class ReservationTests
    {
        [Fact]
        public void IsConfirmed_WhenStatusIsConfirmed_ShouldReturnTrue()
        {
            // Arrange
            var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), "Confirmed");

            // Act
            var result = reservation.IsConfirmed();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsConfirmed_WhenStatusIsNotConfirmed_ShouldReturnFalse()
        {
            // Arrange
            var reservation = new Reservation(Guid.NewGuid(), Guid.NewGuid(), "Pending");

            // Act
            var result = reservation.IsConfirmed();

            // Assert
            Assert.False(result);
        }
    }
}
