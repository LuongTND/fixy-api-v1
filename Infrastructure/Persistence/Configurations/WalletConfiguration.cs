using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.OwnerType).IsRequired();

        builder.Property(x => x.Balance).HasDefaultValue(0);

        builder.Property(x => x.LifetimeEarned).HasDefaultValue(0);

        builder.Property(x => x.LifetimeSpent).HasDefaultValue(0);

        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.UpdatedAt).IsRequired(false);

        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

        builder
            .HasMany(x => x.Transactions)
            .WithOne(x => x.Wallet)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
    }
}
