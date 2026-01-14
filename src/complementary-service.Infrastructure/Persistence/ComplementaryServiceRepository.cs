using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Repositories;
using ComplementaryServices.Domain.Common;

namespace ComplementaryServices.Infrastructure.Persistence
{
    public class ComplementaryServiceRepository : IComplementaryServiceRepository, IUnitOfWork
    {
        private readonly List<ComplementaryService> _services = new();

        public IUnitOfWork UnitOfWork => this;

        public Task AddAsync(ComplementaryService service, CancellationToken cancellationToken)
        {
            _services.Add(service);
            return Task.CompletedTask;
        }

        public Task<ComplementaryService> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var service = _services.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(service);
        }

        public Task<List<ComplementaryService>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var services = _services.Where(s => s.UserId == userId).ToList();
            return Task.FromResult(services);
        }

        public Task UpdateAsync(ComplementaryService service, CancellationToken cancellationToken)
        {
            var index = _services.FindIndex(s => s.Id == service.Id);
            if (index != -1)
            {
                _services[index] = service;
            }
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }
}
