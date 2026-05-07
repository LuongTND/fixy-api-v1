using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.UserId).IsUnique();

            builder.HasOne(x => x.User)
                .WithOne(x => x.Wallet)
                .HasForeignKey<Wallet>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
