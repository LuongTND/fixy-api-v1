using Application.Common;

namespace Application.DTOs.User
{
    public class UserManagementQuery : PagedQuery
    {
        public string? Search { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}
