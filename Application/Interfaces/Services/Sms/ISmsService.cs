namespace Application.Interfaces.Services.Sms
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    }
}
