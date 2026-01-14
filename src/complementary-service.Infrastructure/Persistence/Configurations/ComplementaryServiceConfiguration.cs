using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ComplementaryServices.Domain.Entities;
using ComplementaryServices.Domain.ValueObjects;

namespace ComplementaryServices.Infrastructure.Persistence.Configurations
{
    public class ComplementaryServiceConfiguration : IEntityTypeConfiguration<ComplementaryService>
    {
        public void Configure(EntityTypeBuilder<ComplementaryService> builder)
        {
            builder.ToTable("ComplementaryServices");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.ReservationId)
                .IsRequired();

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.EventId)
                .IsRequired();

            // Value Object: ServiceType
            builder.OwnsOne(s => s.ServiceType, st =>
            {
                st.Property(t => t.Value)
                    .HasColumnName("ServiceType")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Value Object: ServiceStatus
            builder.OwnsOne(s => s.Status, ss =>
            {
                ss.Property(t => t.Value)
                    .HasColumnName("Status")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Value Object: ProviderResponse (puede ser null)
            builder.OwnsOne(s => s.ProviderResponse, pr =>
            {
                pr.Property(p => p.IsAvailable)
                    .HasColumnName("ProviderIsAvailable");

                pr.Property(p => p.ProviderId)
                    .HasColumnName("ProviderResponseId")
                    .HasMaxLength(100);

                pr.Property(p => p.Message)
                    .HasColumnName("ProviderMessage")
                    .HasMaxLength(500);

                pr.Property(p => p.Price)
                    .HasColumnName("ProviderPrice")
                    .HasPrecision(18, 2);

                pr.Property(p => p.EstimatedTime)
                    .HasColumnName("ProviderEstimatedTime");
            });

            builder.Property(s => s.ProviderId)
                .HasMaxLength(100);

            builder.Property(s => s.Price)
                .HasPrecision(18, 2);

            builder.Property(s => s.Details)
                .HasMaxLength(1000);

            builder.Property(s => s.RejectionReason)
                .HasMaxLength(500);

            builder.Property(s => s.RequestedAt)
                .IsRequired();

            // Ignorar DomainEvents (no se persisten)
            builder.Ignore(s => s.DomainEvents);

            // Índices
            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.ReservationId);
            builder.HasIndex(s => s.EventId);
            builder.HasIndex(s => new { s.UserId, s.ReservationId });
        }
    }
}
