using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Application.DTOs;
using ComplementaryServices.Application.Exceptions;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Repositories;
using ComplementaryServices.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplementaryServices.Application.Services
{
    public interface IComplementaryServiceAppService
    {
        Task<Guid> RequestServiceAsync(ServiceRequestDto request, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ConfirmServiceAsync(Guid serviceId, string providerId, decimal price, string message, DateTime? estimatedTime = null, CancellationToken cancellationToken = default);
        Task<bool> RejectServiceAsync(Guid serviceId, string reason, CancellationToken cancellationToken = default);
        Task<List<ServiceStatusDto>> GetUserServicesAsync(Guid userId, Guid? reservationId = null, CancellationToken cancellationToken = default);
        Task<ServiceStatusDto> GetServiceByIdAsync(Guid serviceId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> CancelServiceAsync(Guid serviceId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<ServiceStatusDto>> GetServicesByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task<ServiceMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
    }

    public class ComplementaryServiceAppService : IComplementaryServiceAppService
    {
        private readonly IComplementaryServiceRepository _repository;
        private readonly IReservationRepository _reservationRepository;
        private readonly ILogger<ComplementaryServiceAppService> _logger;
        private readonly IMediator _mediator;

        public ComplementaryServiceAppService(
            IComplementaryServiceRepository repository,
            IReservationRepository reservationRepository,
            ILogger<ComplementaryServiceAppService> logger,
            IMediator mediator)
        {
            _repository = repository;
            _reservationRepository = reservationRepository;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Guid> RequestServiceAsync(ServiceRequestDto request, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(request.ReservationId, cancellationToken);
                if (reservation == null)
                {
                    throw new ReservationNotFoundException(request.ReservationId);
                }

                if (reservation.UserId != userId)
                {
                    throw new UnauthorizedAccessException("User does not own this reservation");
                }

                if (!reservation.IsConfirmed())
                {
                    throw new InvalidServiceRequestException("Cannot request services for unconfirmed reservation");
                }

                var serviceType = ServiceType.FromString(request.ServiceType);

                var service = new ComplementaryService(
                    request.ReservationId,
                    userId,
                    request.EventId,
                    serviceType,
                    request.Details);

                await _repository.AddAsync(service, cancellationToken);
                await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Service {ServiceId} of type {ServiceType} requested for reservation {ReservationId}",
                    service.Id,
                    serviceType.Value,
                    request.ReservationId);

                await PublishDomainEventsAsync(service, cancellationToken);

                return service.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting service for reservation {ReservationId}", request.ReservationId);
                throw;
            }
        }

        public async Task<bool> ConfirmServiceAsync(Guid serviceId, string providerId, decimal price, string message, DateTime? estimatedTime = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await _repository.GetByIdAsync(serviceId, cancellationToken);
                if (service == null)
                {
                    throw new ServiceNotFoundException(serviceId);
                }

                var providerResponse = new ProviderResponse(
                    isAvailable: true,
                    providerId: providerId,
                    message: message,
                    price: price,
                    estimatedTime: estimatedTime);

                service.Confirm(providerResponse);

                await _repository.UpdateAsync(service, cancellationToken);
                await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Service {ServiceId} confirmed by provider {ProviderId} with price {Price}",
                    service.Id,
                    providerId,
                    price);

                await PublishDomainEventsAsync(service, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming service {ServiceId}", serviceId);
                throw;
            }
        }

        public async Task<bool> RejectServiceAsync(Guid serviceId, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await _repository.GetByIdAsync(serviceId, cancellationToken);
                if (service == null)
                {
                    throw new ServiceNotFoundException(serviceId);
                }

                service.Reject(reason);

                await _repository.UpdateAsync(service, cancellationToken);
                await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Service {ServiceId} rejected: {Reason}", serviceId, reason);

                await PublishDomainEventsAsync(service, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting service {ServiceId}", serviceId);
                throw;
            }
        }

        public async Task<List<ServiceStatusDto>> GetUserServicesAsync(Guid userId, Guid? reservationId = null, CancellationToken cancellationToken = default)
        {
            var services = await _repository.GetByUserIdAsync(userId, cancellationToken);

            if (reservationId.HasValue)
            {
                services = services.Where(s => s.ReservationId == reservationId.Value).ToList();
            }

            return services.Select(MapToDto).ToList();
        }

        public async Task<ServiceStatusDto> GetServiceByIdAsync(Guid serviceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var service = await _repository.GetByIdAsync(serviceId, cancellationToken);
            if (service == null) throw new ServiceNotFoundException(serviceId);
            if (service.UserId != userId) throw new UnauthorizedAccessException();

            return MapToDto(service);
        }

        public async Task<bool> CancelServiceAsync(Guid serviceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var service = await _repository.GetByIdAsync(serviceId, cancellationToken);
            if (service == null) throw new ServiceNotFoundException(serviceId);
            if (service.UserId != userId) throw new UnauthorizedAccessException();

            service.Cancel();
            await _repository.UpdateAsync(service, cancellationToken);
            await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Service {ServiceId} cancelled by user {UserId}", serviceId, userId);
            await PublishDomainEventsAsync(service, cancellationToken);
            return true;
        }

        public async Task<List<ServiceStatusDto>> GetServicesByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            var services = await _repository.GetByEventIdAsync(eventId, cancellationToken);
            return services.Select(MapToDto).ToList();
        }

        public async Task<ServiceMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
        {
            // Nota: En una app real esto sería una query SQL optimizada o un ReadModel
            var allServices = await _repository.GetAllAsync(cancellationToken); 
            
            return new ServiceMetricsDto
            {
                TotalRequests = allServices.Count,
                Confirmed = allServices.Count(s => s.Status == ServiceStatus.Confirmed),
                Rejected = allServices.Count(s => s.Status == ServiceStatus.Rejected),
                Pending = allServices.Count(s => s.Status == ServiceStatus.Pending || s.Status == ServiceStatus.Requested),
                AveragePrice = allServices.Any(s => s.Price > 0) ? allServices.Where(s => s.Price > 0).Average(s => s.Price) : 0,
                ByServiceType = allServices.GroupBy(s => s.ServiceType.Value).ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private ServiceStatusDto MapToDto(ComplementaryService s)
        {
            return new ServiceStatusDto
            {
                ServiceId = s.Id,
                ReservationId = s.ReservationId,
                ServiceType = s.ServiceType.Value,
                Status = s.Status.Value,
                ProviderId = s.ProviderId,
                Price = s.Price,
                RequestedAt = s.RequestedAt,
                ConfirmedAt = s.ConfirmedAt,
                RejectedAt = s.RejectedAt,
                RejectionReason = s.RejectionReason,
                Details = s.Details
            };
        }

        private async Task PublishDomainEventsAsync(ComplementaryService service, CancellationToken cancellationToken)
        {
            foreach (var domainEvent in service.DomainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            service.ClearDomainEvents();
        }
    }
}
