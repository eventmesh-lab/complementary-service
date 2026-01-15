using System;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.EventHandlers;
using ComplementaryServices.Application.Notifications;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ComplementaryServices.Application.Tests.EventHandlers
{
    public class ServiceConfirmedEventHandlerTests
    {
        [Fact]
        public async Task Handle_WhenEventReceived_ShouldNotifyUser()
        {
            // Arrange
            var notifierMock = new Mock<IServiceNotifier>();
            var loggerMock = new Mock<ILogger<ServiceConfirmedEventHandler>>();
            var handler = new ServiceConfirmedEventHandler(notifierMock.Object, loggerMock.Object);

            var domainEvent = new ServiceConfirmedDomainEvent(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                ServiceType.Catering, "p1", 100m);

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert
            notifierMock.Verify(n => n.NotifyServiceConfirmedAsync(
                domainEvent.UserId,
                domainEvent.ServiceId,
                domainEvent.ServiceType.Value,
                domainEvent.ProviderId,
                domainEvent.Price,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class ServiceRejectedEventHandlerTests
    {
        [Fact]
        public async Task Handle_WhenEventReceived_ShouldNotifyUser()
        {
            // Arrange
            var notifierMock = new Mock<IServiceNotifier>();
            var loggerMock = new Mock<ILogger<ServiceRejectedEventHandler>>();
            var handler = new ServiceRejectedEventHandler(notifierMock.Object, loggerMock.Object);

            var domainEvent = new ServiceRejectedDomainEvent(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                ServiceType.Catering, "Out of stock");

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert
            notifierMock.Verify(n => n.NotifyServiceRejectedAsync(
                domainEvent.UserId,
                domainEvent.ServiceId,
                domainEvent.ServiceType.Value,
                domainEvent.RejectionReason,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
