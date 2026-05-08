using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class PlatformConfig : BaseEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public PlatformConfigType Type { get; set; }
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? Description { get; set; }
        public Guid UpdatedById { get; set; }

        public User? UpdatedBy { get; set; }
    }
}
