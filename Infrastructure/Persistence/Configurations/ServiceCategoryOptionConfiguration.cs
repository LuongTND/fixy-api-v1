using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ServiceCategoryOptionConfiguration : IEntityTypeConfiguration<ServiceCategoryOption>
    {
        public void Configure(EntityTypeBuilder<ServiceCategoryOption> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DurationMinutes).IsRequired();
            builder.Property(x => x.Price).IsRequired();

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
