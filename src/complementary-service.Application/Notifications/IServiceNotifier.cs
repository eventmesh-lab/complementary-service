using System;
using System.Threading;
using System.Threading.Tasks;

namespace ComplementaryServices.Application.Notifications
{
    /// <summary>
    /// TC-062: Interface para notificaciones en tiempo real
    /// </summary>
    public interface IServiceNotifier
    {
        Task NotifyServiceConfirmedAsync(
            Guid userId,
            Guid serviceId,
            string serviceType,
            string providerId,
            decimal price,
            CancellationToken cancellationToken = default);

        Task NotifyServiceRejectedAsync(
            Guid userId,
            Guid serviceId,
            string serviceType,
            string reason,
            CancellationToken cancellationToken = default);

        Task NotifyServiceUpdatedAsync(
            Guid userId,
            Guid serviceId,
            string status,
            string message,
            CancellationToken cancellationToken = default);
    }
}
