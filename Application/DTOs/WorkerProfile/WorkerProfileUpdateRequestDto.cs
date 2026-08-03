using Application.DTOs.Address;
using Application.DTOs.WorkerProfile.WorkerService;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.WorkerProfile
{
    public class WorkerProfileUpdateRequestDto
    {
        public string? Phone { get; set; }
        public string? Bio { get; set; }

        public int? ExperienceYears { get; set; }
        public int? MaxDistanceKm { get; set; }
        public IFormFile? Avatar { get; set; }

        public UpdateAddressRequestDto? Address { get; set; }

        public List<WorkerServiceUpdateRequestDto>? Services { get; set; }
    }
}
