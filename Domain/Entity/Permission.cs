using Domain.Common;

namespace Domain.Entity
{
    public class Permission : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? GroupName { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
