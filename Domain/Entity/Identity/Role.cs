using Domain.Common;

namespace Domain.Entity.Identity
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = default!;

        public string Code { get; set; } = default!;

        // Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
