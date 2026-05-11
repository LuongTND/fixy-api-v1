using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
    {
        public void Configure(EntityTypeBuilder<EventLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EventType).IsRequired();
            builder.Property(x => x.ActorType).HasConversion<string>();
            builder.Property(x => x.EntityType).HasConversion<string>();
            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(x => x.Payload).IsRequired();
            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedDate }).HasDatabaseName("idx_event_entity");
            builder.HasIndex(x => new { x.ActorId, x.CreatedDate }).HasDatabaseName("idx_event_actor");
            builder.HasIndex(x => new { x.EventType, x.CreatedDate }).HasDatabaseName("idx_event_type");
            builder.HasIndex(x => x.CreatedDate).HasDatabaseName("idx_event_created");
        }
    }
}
