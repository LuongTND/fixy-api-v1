namespace Application.DTOs.Profile
{
    public class UpdateProfileRequestDto
    {
        public string FullName { get; set; } = default!;

        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Gender { get; set; }
    }
}
