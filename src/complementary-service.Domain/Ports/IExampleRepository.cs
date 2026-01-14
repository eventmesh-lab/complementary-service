using System;
using System.Threading;
using System.Threading.Tasks;
using complementary_service.Domain.Entities;

namespace complementary_service.Domain.Ports
{
    public interface IExampleRepository
    {
        Task AddAsync(ExampleAggregate entity, CancellationToken cancellationToken = default);
        Task<ExampleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
