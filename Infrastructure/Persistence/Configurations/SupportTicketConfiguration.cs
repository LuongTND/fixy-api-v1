using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
    {
        public void Configure(EntityTypeBuilder<SupportTicket> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ReporterType).HasConversion<string>();
            builder.Property(x => x.Category).HasConversion<string>();
            builder.Property(x => x.Priority).HasConversion<string>();
            builder.Property(x => x.Status).HasConversion<string>();
            builder.Property(x => x.Subject).IsRequired();
            builder.HasIndex(x => new { x.Status, x.Priority, x.CreatedDate }).HasDatabaseName("idx_ticket_queue");
            builder.HasIndex(x => x.ReporterId).HasDatabaseName("idx_ticket_reporter");

            builder.HasOne(x => x.Reporter)
                .WithMany()
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany(x => x.SupportTickets)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedTo)
                .WithMany()
                .HasForeignKey(x => x.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
