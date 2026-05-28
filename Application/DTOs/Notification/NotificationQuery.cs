namespace Application.DTOs.Notification
{
    public class NotificationQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsRead { get; set; }
    }
}
