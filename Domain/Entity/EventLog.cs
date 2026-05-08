using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class EventLog : BaseEntity
    {
        public string EventType { get; set; } = string.Empty;
        public Guid? ActorId { get; set; }
        public EventActorType ActorType { get; set; }
        public EventEntityType EntityType { get; set; }
        public string EntityId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public User? Actor { get; set; }
    }
}
