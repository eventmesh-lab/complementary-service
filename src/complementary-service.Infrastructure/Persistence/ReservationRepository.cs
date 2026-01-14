using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.Repositories;

namespace ComplementaryServices.Infrastructure.Persistence
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly List<Reservation> _reservations = new()
        {
            // Seed some reservations for testing
            new Reservation(Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("11111111-1111-1111-1111-111111111111"), "Confirmed"),
            new Reservation(Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("22222222-2222-2222-2222-222222222222"), "Pending")
        };

        public Task<Reservation> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var reservation = _reservations.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(reservation);
        }
    }
}
