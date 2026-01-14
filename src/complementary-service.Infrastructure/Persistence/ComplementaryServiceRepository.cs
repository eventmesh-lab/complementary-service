using Microsoft.EntityFrameworkCore;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComplementaryServices.Infrastructure.Persistence
{
    public class ComplementaryServiceRepository : IComplementaryServiceRepository
    {
        private readonly ComplementaryServiceDbContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public ComplementaryServiceRepository(ComplementaryServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ComplementaryService> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ComplementaryServices
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<List<ComplementaryService>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ComplementaryServices
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ComplementaryService>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
        {
            return await _context.ComplementaryServices
                .Where(s => s.ReservationId == reservationId)
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ComplementaryService>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            return await _context.ComplementaryServices
                .Where(s => s.EventId == eventId)
                .OrderByDescending(s => s.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ComplementaryService service, CancellationToken cancellationToken = default)
        {
            await _context.ComplementaryServices.AddAsync(service, cancellationToken);
        }

        public Task UpdateAsync(ComplementaryService service, CancellationToken cancellationToken = default)
        {
            _context.ComplementaryServices.Update(service);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ComplementaryService service, CancellationToken cancellationToken = default)
        {
            _context.ComplementaryServices.Remove(service);
            return Task.CompletedTask;
        }
    }
}
