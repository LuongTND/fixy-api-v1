using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PlatformConfigConfiguration : IEntityTypeConfiguration<PlatformConfig>
    {
        public void Configure(EntityTypeBuilder<PlatformConfig> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Key).IsRequired();
            builder.Property(x => x.Value).IsRequired();
            builder.Property(x => x.Type).HasConversion<string>();
            builder.HasIndex(x => new { x.Key, x.IsActive }).HasDatabaseName("idx_config_active");
            builder.HasIndex(x => x.Key).HasDatabaseName("idx_config_key");
        }
    }
}
