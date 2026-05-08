using Domain.Common;

namespace Domain.Entity
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<RolePermission> RolePermissions { get; set; } =
            new List<RolePermission>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
