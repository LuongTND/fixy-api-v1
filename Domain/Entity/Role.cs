using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class Role : BaseEntity
    {
        public RoleName Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
