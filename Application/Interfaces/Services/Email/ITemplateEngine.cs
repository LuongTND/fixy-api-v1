using Application.Common.Models.Email;

namespace Application.Interfaces.Services.Email
{
    public interface ITemplateEngine
    {
        Task<string> RenderEmailTemplateAsync(string template, OtpEmailModel model);
    }
}
