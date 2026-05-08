namespace Domain.Entity
{
    public class NotificationSetting
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool NewBooking { get; set; } = true;
        public bool Payment { get; set; } = true;
        public bool StatusUpdate { get; set; } = true;
        public bool Promotions { get; set; } = true;
        public bool ViaPush { get; set; } = true;
        public bool ViaSms { get; set; }
        public bool ViaEmail { get; set; }
        public bool ViaInApp { get; set; } = true;

        public User? User { get; set; }
    }
}
