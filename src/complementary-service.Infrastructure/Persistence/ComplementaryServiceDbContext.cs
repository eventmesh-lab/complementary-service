using Microsoft.EntityFrameworkCore;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Infrastructure.Persistence.Configurations;
using System.Threading;
using System.Threading.Tasks;
using ComplementaryServices.Domain.Repositories;

namespace ComplementaryServices.Infrastructure.Persistence
{
    public class ComplementaryServiceDbContext : DbContext, IUnitOfWork
    {
        public DbSet<ComplementaryService> ComplementaryServices { get; set; }
        public DbSet<ServiceProvider> ServiceProviders { get; set; }

        public ComplementaryServiceDbContext(DbContextOptions<ComplementaryServiceDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones
            modelBuilder.ApplyConfiguration(new ComplementaryServiceConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceProviderConfiguration());
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
