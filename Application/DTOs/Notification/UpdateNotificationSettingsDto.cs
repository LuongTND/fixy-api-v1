namespace Application.DTOs.Notification
{
    public class UpdateNotificationSettingsDto
    {
        public bool? NewBooking { get; set; }
        public bool? Payment { get; set; }
        public bool? StatusUpdate { get; set; }
        public bool? Promotions { get; set; }
        public bool? ViaPush { get; set; }
        public bool? ViaInApp { get; set; }
    }
}
