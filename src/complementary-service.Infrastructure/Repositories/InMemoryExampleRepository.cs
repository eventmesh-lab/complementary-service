using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using complementary_service.Domain.Entities;
using complementary_service.Domain.Ports;

namespace complementary_service.Infrastructure.Repositories
{
    public class InMemoryExampleRepository : IExampleRepository
    {
        private readonly ConcurrentDictionary<Guid, ExampleAggregate> _store = new();

        public Task AddAsync(ExampleAggregate entity, CancellationToken cancellationToken = default)
        {
            _store[entity.Id] = entity;
            return Task.CompletedTask;
        }

        public Task<ExampleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }
    }
}
