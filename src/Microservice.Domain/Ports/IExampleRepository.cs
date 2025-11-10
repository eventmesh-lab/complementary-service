using System;
using System.Threading;
using System.Threading.Tasks;
using Microservice.Domain.Entities;

namespace Microservice.Domain.Ports
{
    public interface IExampleRepository
    {
        Task AddAsync(ExampleAggregate entity, CancellationToken cancellationToken = default);
        Task<ExampleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
