namespace Application.Interfaces.Services.Email
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp, CancellationToken cancellationToken = default);
    }
}
