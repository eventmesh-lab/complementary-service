using System;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.EventHandlers;
using ComplementaryServices.Application.Messaging.RabbitMQ;
using ComplementaryServices.Domain.Events;
using ComplementaryServices.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ComplementaryServices.Application.Tests.EventHandlers
{
    public class ServiceRequestedEventHandlerTests
    {
        [Fact]
        public async Task Handle_WhenEventReceived_ShouldPublishToQueue()
        {
            // Arrange
            var publisherMock = new Mock<IServiceRequestPublisher>();
            var loggerMock = new Mock<ILogger<ServiceRequestedEventHandler>>();
            var handler = new ServiceRequestedEventHandler(publisherMock.Object, loggerMock.Object);

            var domainEvent = new ServiceRequestedDomainEvent(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                ServiceType.Catering, "Details");

            // Act
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert
            publisherMock.Verify(p => p.PublishServiceRequestAsync(
                domainEvent.ServiceId,
                domainEvent.ReservationId,
                domainEvent.EventId,
                domainEvent.ServiceType,
                domainEvent.Details,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
