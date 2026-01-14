using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Domain.Repositories
{
    public interface IComplementaryServiceRepository
    {
        IUnitOfWork UnitOfWork { get; }
        Task<ComplementaryService> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<ComplementaryService>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task AddAsync(ComplementaryService service, CancellationToken cancellationToken);
        Task UpdateAsync(ComplementaryService service, CancellationToken cancellationToken);
    }
}
