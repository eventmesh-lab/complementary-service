using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.DTOs;
using ComplementaryServices.Application.Exceptions;
using ComplementaryServices.Application.Services;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Repositories;
using ComplementaryServices.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ComplementaryServices.Application.Tests.Services
{
    public class ComplementaryServiceAppServiceTests
    {
        private readonly Mock<IComplementaryServiceRepository> _repositoryMock;
        private readonly Mock<IReservationRepository> _reservationRepositoryMock;
        private readonly Mock<ILogger<ComplementaryServiceAppService>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ComplementaryServiceAppService _service;

        public ComplementaryServiceAppServiceTests()
        {
            _repositoryMock = new Mock<IComplementaryServiceRepository>();
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _loggerMock = new Mock<ILogger<ComplementaryServiceAppService>>();
            _mediatorMock = new Mock<IMediator>();

            // Mock UnitOfWork
            var uowMock = new Mock<IUnitOfWork>();
            _repositoryMock.Setup(r => r.UnitOfWork).Returns(uowMock.Object);

            _service = new ComplementaryServiceAppService(
                _repositoryMock.Object,
                _reservationRepositoryMock.Object,
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task RequestServiceAsync_WhenValidRequest_ShouldReturnServiceId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new ServiceRequestDto
            {
                ReservationId = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                ServiceType = "Catering",
                Details = "Details"
            };

            var reservation = new Reservation(request.ReservationId, userId, "Confirmed");
            _reservationRepositoryMock.Setup(r => r.GetByIdAsync(request.ReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            // Act
            var result = await _service.RequestServiceAsync(request, userId);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ComplementaryService>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RequestServiceAsync_WhenReservationNotFound_ShouldThrowReservationNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new ServiceRequestDto { ReservationId = Guid.NewGuid() };
            _reservationRepositoryMock.Setup(r => r.GetByIdAsync(request.ReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Reservation)null);

            // Act & Assert
            await Assert.ThrowsAsync<ReservationNotFoundException>(() => _service.RequestServiceAsync(request, userId));
        }

        [Fact]
        public async Task RequestServiceAsync_WhenUserDoesNotOwnReservation_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new ServiceRequestDto { ReservationId = Guid.NewGuid() };
            var reservation = new Reservation(request.ReservationId, otherUserId, "Confirmed");
            _reservationRepositoryMock.Setup(r => r.GetByIdAsync(request.ReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.RequestServiceAsync(request, userId));
        }

        [Fact]
        public async Task ConfirmServiceAsync_WhenValid_ShouldReturnTrue()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Catering, "Details");
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Act
            var result = await _service.ConfirmServiceAsync(serviceId, "p1", 100m, "OK");

            // Assert
            Assert.True(result);
            Assert.Equal(ServiceStatus.Confirmed, service.Status);
            _repositoryMock.Verify(r => r.UpdateAsync(service, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RejectServiceAsync_WhenValid_ShouldReturnTrue()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            var service = new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Catering, "Details");
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Act
            var result = await _service.RejectServiceAsync(serviceId, "No capacity");

            // Assert
            Assert.True(result);
            Assert.Equal(ServiceStatus.Rejected, service.Status);
        }

        [Fact]
        public async Task CancelServiceAsync_WhenUserOwnsService_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var service = new ComplementaryService(Guid.NewGuid(), userId, Guid.NewGuid(), ServiceType.Catering, "Details");
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Act
            var result = await _service.CancelServiceAsync(serviceId, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(ServiceStatus.Cancelled, service.Status);
        }

        [Fact]
        public async Task GetMetricsAsync_ShouldReturnPopulatedMetrics()
        {
            // Arrange
            var services = new List<ComplementaryService>
            {
                new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Catering, "D1"),
                new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ServiceType.Transport, "D2")
            };
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Act
            var result = await _service.GetMetricsAsync();

            // Assert
            Assert.Equal(2, result.TotalRequests);
            Assert.Equal(2, result.Pending); // Requested is counted as pending in logic
            Assert.True(result.ByServiceType.ContainsKey("Catering"));
        }

        [Fact]
        public async Task GetMetricsAsync_WhenEmpty_ShouldReturnZeroMetrics()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ComplementaryService>());

            // Act
            var result = await _service.GetMetricsAsync();

            // Assert
            Assert.Equal(0, result.TotalRequests);
            Assert.Equal(0, result.AveragePrice);
            Assert.Empty(result.ByServiceType);
        }

        [Fact]
        public async Task GetUserServicesAsync_WithReservationId_ShouldFilterResults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var res1 = Guid.NewGuid();
            var res2 = Guid.NewGuid();
            var services = new List<ComplementaryService>
            {
                new ComplementaryService(res1, userId, Guid.NewGuid(), ServiceType.Catering, "D1"),
                new ComplementaryService(res2, userId, Guid.NewGuid(), ServiceType.Transport, "D2")
            };
            _repositoryMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Act
            var result = await _service.GetUserServicesAsync(userId, res1);

            // Assert
            Assert.Single(result);
            Assert.Equal(res1, result[0].ReservationId);
        }

        [Fact]
        public async Task GetServiceByIdAsync_WhenUserOwnsService_ShouldReturnDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var service = new ComplementaryService(Guid.NewGuid(), userId, Guid.NewGuid(), ServiceType.Catering, "Details");
            var serviceId = service.Id;
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Act
            var result = await _service.GetServiceByIdAsync(serviceId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(serviceId, result.ServiceId);
        }

        [Fact]
        public async Task GetServiceByIdAsync_WhenUserDoesNotOwnService_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var serviceId = Guid.NewGuid();
            var service = new ComplementaryService(Guid.NewGuid(), otherUserId, Guid.NewGuid(), ServiceType.Catering, "Details");
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.GetServiceByIdAsync(serviceId, userId));
        }

        [Fact]
        public async Task RequestServiceAsync_WhenReservationIsNotConfirmed_ShouldThrowInvalidServiceRequestException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new ServiceRequestDto { ReservationId = Guid.NewGuid(), ServiceType = "Catering" };
            var reservation = new Reservation(request.ReservationId, userId, "Pending");
            _reservationRepositoryMock.Setup(r => r.GetByIdAsync(request.ReservationId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reservation);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidServiceRequestException>(() => _service.RequestServiceAsync(request, userId));
        }

        [Fact]
        public async Task ConfirmServiceAsync_WhenServiceNotFound_ShouldThrowServiceNotFoundException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ComplementaryService)null);

            // Act & Assert
            await Assert.ThrowsAsync<ServiceNotFoundException>(() => _service.ConfirmServiceAsync(serviceId, "p1", 10m, "OK"));
        }

        [Fact]
        public async Task RejectServiceAsync_WhenServiceNotFound_ShouldThrowServiceNotFoundException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ComplementaryService)null);

            // Act & Assert
            await Assert.ThrowsAsync<ServiceNotFoundException>(() => _service.RejectServiceAsync(serviceId, "No reason"));
        }

        [Fact]
        public async Task CancelServiceAsync_WhenServiceNotFound_ShouldThrowServiceNotFoundException()
        {
            // Arrange
            var serviceId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ComplementaryService)null);

            // Act & Assert
            await Assert.ThrowsAsync<ServiceNotFoundException>(() => _service.CancelServiceAsync(serviceId, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetServicesByEventAsync_ShouldReturnList()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var services = new List<ComplementaryService>
            {
                new ComplementaryService(Guid.NewGuid(), Guid.NewGuid(), eventId, ServiceType.Catering, "D1")
            };
            _repositoryMock.Setup(r => r.GetByEventIdAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Act
            var result = await _service.GetServicesByEventAsync(eventId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Catering", result[0].ServiceType);
        }
    }
}
