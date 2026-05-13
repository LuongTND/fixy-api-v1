using Domain.Enum;

namespace Application.DTOs.Profile
{
    public class UpdateProfileRequestDto
    {
        public string FullName { get; set; } = default!;

        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public Gender Gender { get; set; }
    }
}
