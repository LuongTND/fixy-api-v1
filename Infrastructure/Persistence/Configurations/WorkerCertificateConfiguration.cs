using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerCertificateConfiguration : IEntityTypeConfiguration<WorkerCertificate>
    {
        public void Configure(EntityTypeBuilder<WorkerCertificate> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).IsRequired();
            builder.HasIndex(x => x.WorkerId).HasDatabaseName("idx_cert_worker");

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.Certificates)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
