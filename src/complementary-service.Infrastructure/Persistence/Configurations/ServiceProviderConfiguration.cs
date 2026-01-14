using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ComplementaryServices.Domain.Entities;

namespace ComplementaryServices.Infrastructure.Persistence.Configurations
{
    public class ServiceProviderConfiguration : IEntityTypeConfiguration<ServiceProvider>
    {
        public void Configure(EntityTypeBuilder<ServiceProvider> builder)
        {
            builder.ToTable("ServiceProviders");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.OwnsOne(p => p.ServiceType, st =>
            {
                st.Property(t => t.Value)
                    .HasColumnName("ServiceType")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            builder.Property(p => p.ApiEndpoint)
                .HasMaxLength(500);

            builder.Property(p => p.QueueName)
                .HasMaxLength(200);

            builder.Property(p => p.IsActive)
                .IsRequired();

            builder.Property(p => p.Priority)
                .IsRequired();

            builder.HasIndex(p => p.IsActive);
        }
    }
}
