using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WorkerPayoutAccountConfiguration : IEntityTypeConfiguration<WorkerPayoutAccount>
    {
        public void Configure(EntityTypeBuilder<WorkerPayoutAccount> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Method).HasConversion<string>();
            builder.Property(x => x.AccountNumber).IsRequired();
            builder.Property(x => x.AccountName).IsRequired();
            builder.HasIndex(x => new { x.WorkerId, x.IsDefault }).HasDatabaseName("idx_payout_acct_worker");

            builder.HasOne(x => x.Worker)
                .WithMany(x => x.PayoutAccounts)
                .HasForeignKey(x => x.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
