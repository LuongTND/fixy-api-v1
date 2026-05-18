using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class Media : BaseEntity, ISoftDelete
    {
        public Guid OwnerId { get; set; }
        public MediaOwnerType OwnerType { get; set; }
        public MediaCategory Category { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsDeleted { get; set; }
        public Guid UploadedById { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public User? UploadedBy { get; set; }
    }
}
