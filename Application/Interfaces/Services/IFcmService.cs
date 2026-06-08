namespace Application.Interfaces.Services
{
    public interface IFcmService
    {
        Task<bool> SendPushNotificationAsync(string token,string title,string body,string? deepLink = null,object? meta = null,CancellationToken cancellationToken = default);

        Task SendBatchPushNotificationAsync(List<string> tokens,string title,string body,string? deepLink = null,object? meta = null,CancellationToken cancellationToken = default);
    }
}
