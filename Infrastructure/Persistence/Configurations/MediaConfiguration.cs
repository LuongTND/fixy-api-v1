using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.OwnerType).HasConversion<string>();
            builder.Property(x => x.Category).HasConversion<string>();
            builder.Property(x => x.FileUrl).IsRequired();

            builder
                .HasIndex(x => new
                {
                    x.OwnerType,
                    x.OwnerId,
                    x.Category,
                })
                .HasDatabaseName("idx_media_owner");
            builder
                .HasIndex(x => new
                {
                    x.OwnerType,
                    x.OwnerId,
                    x.SortOrder,
                })
                .HasDatabaseName("idx_media_sort");
            builder.HasIndex(x => x.UploadedById).HasDatabaseName("idx_media_uploader");

            builder
                .HasOne(x => x.UploadedBy)
                .WithMany()
                .HasForeignKey(x => x.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
