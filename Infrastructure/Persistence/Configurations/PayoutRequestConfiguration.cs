using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PayoutRequestConfiguration : IEntityTypeConfiguration<PayoutRequest>
    {
        public void Configure(EntityTypeBuilder<PayoutRequest> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>();
            builder
                .HasIndex(x => new
                {
                    x.WorkerProfileId,
                    x.Status,
                    x.CreatedDate,
                })
                .HasDatabaseName("idx_payout_worker");
            builder
                .HasIndex(x => new { x.Status, x.CreatedDate })
                .HasDatabaseName("idx_payout_status");

            builder
                .HasOne(x => x.Worker)
                .WithMany()
                .HasForeignKey(x => x.WorkerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.PayoutAccount)
                .WithMany(x => x.PayoutRequests)
                .HasForeignKey(x => x.PayoutAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.ReviewedBy)
                .WithMany()
                .HasForeignKey(x => x.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(x => x.WalletTransactions)
                .WithOne(x => x.PayoutRequest)
                .HasForeignKey(x => x.PayoutRequestId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
