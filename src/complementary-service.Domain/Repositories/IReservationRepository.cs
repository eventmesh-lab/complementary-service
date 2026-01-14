using System;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Domain.Entities;

namespace ComplementaryServices.Domain.Repositories
{
    public interface IReservationRepository
    {
        Task<Reservation> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
