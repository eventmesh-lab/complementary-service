using ComplementaryServices.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComplementaryServices.Domain.Repositories
{
    public interface IComplementaryServiceRepository
    {
        IUnitOfWork UnitOfWork { get; }

        Task<ComplementaryService> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ComplementaryService>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<ComplementaryService>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
        Task<List<ComplementaryService>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
        Task AddAsync(ComplementaryService service, CancellationToken cancellationToken = default);
        Task UpdateAsync(ComplementaryService service, CancellationToken cancellationToken = default);
        Task DeleteAsync(ComplementaryService service, CancellationToken cancellationToken = default);
    }

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
