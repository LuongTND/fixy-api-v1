using Domain.Common;
using Domain.Enum;

namespace Domain.Entity.Identity
{
    public class User : BaseEntity, ISoftDelete
    {
        public string FullName { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;

        public string? Email { get; set; }

        public string PasswordHash { get; set; } = default!;

        public DateTime? DateOfBirth { get; set; }

        public Gender Gender { get; set; }

        public UserStatus Status { get; set; } = UserStatus.Pending;

        public bool IsDeleted { get; set; }
        public string? DeletedBy { get; set; }

        public DateTime? DeletedDate { get; set; }

        // Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
