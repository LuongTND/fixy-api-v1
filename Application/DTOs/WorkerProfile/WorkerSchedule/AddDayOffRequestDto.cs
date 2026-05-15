namespace Application.DTOs.WorkerProfile.WorkerSchedule
{
    public class AddDayOffRequestDto
    {
        public DateOnly Date { get; set; }

        public string? Reason { get; set; }
    }
}
