namespace Application.DTOs.WorkerProfile.WorkerSchedule
{
    public class WorkerScheduleExceptionDto
    {
        public Guid Id { get; set; }

        public DateOnly Date { get; set; }

        public bool IsDayOff { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public string? Reason { get; set; }
    }
}
