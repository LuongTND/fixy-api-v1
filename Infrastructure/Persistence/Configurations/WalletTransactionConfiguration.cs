using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).IsRequired();

        builder.Property(x => x.Type).IsRequired();

        builder.Property(x => x.Direction).IsRequired();

        builder.Property(x => x.Status).IsRequired();

        builder.Property(x => x.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => x.WalletId);
        builder
            .HasIndex(x => x.ExternalTransactionId)
            .IsUnique()
            .HasFilter("[ExternalTransactionId] IS NOT NULL");
        builder.HasIndex(x => x.ReferenceId);

        builder.HasOne(x => x.Wallet).WithMany(x => x.Transactions).HasForeignKey(x => x.WalletId);
        builder
            .HasOne(x => x.PaymentOrder)
            .WithMany(x => x.WalletTransactions)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(x => x.PayoutRequest)
            .WithMany(x => x.WalletTransactions)
            .HasForeignKey(x => x.PayoutRequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
