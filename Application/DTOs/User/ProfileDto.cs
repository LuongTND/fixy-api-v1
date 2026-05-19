namespace Application.DTOs.Profile
{
    public class ProfileDto
    {
        public string FullName { get; set; } = default!;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
