namespace Application.Settings
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string? InstanceName { get; set; }
    }
}
