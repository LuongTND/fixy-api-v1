using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SupportMessageConfiguration : IEntityTypeConfiguration<SupportMessage>
    {
        public void Configure(EntityTypeBuilder<SupportMessage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Content).IsRequired();
            builder.HasIndex(x => new { x.TicketId, x.CreatedDate }).HasDatabaseName("idx_smsg_ticket");

            builder.HasOne(x => x.Ticket)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
