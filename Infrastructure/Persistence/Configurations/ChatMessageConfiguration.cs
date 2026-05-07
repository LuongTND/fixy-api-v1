using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).HasConversion<string>();
            builder.HasIndex(x => new { x.BookingId, x.CreatedDate }).HasDatabaseName("idx_chat_booking");
            builder.HasIndex(x => new { x.BookingId, x.IsRead }).HasDatabaseName("idx_chat_unread");

            builder.HasOne(x => x.Booking)
                .WithMany(x => x.ChatMessages)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
