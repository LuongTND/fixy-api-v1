using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasConversion<string>();
            builder.Property(x => x.Direction).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
            builder.HasIndex(x => new { x.WalletId, x.CreatedDate }).HasDatabaseName("idx_wtxn_wallet");
            builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId }).HasDatabaseName("idx_wtxn_ref");
            builder.HasIndex(x => new { x.Status, x.CreatedDate }).HasDatabaseName("idx_wtxn_status");

            builder.HasOne(x => x.Wallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReversedBy)
                .WithMany()
                .HasForeignKey(x => x.ReversedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
