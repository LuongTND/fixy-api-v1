namespace Application.DTOs.User
{
    public class UserManagementDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string Phone { get; set; } = default!;

        public string Role { get; set; } = default!;

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
