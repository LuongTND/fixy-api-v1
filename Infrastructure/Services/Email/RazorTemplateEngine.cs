using Application.Common.Models.Email;
using Application.Interfaces.Services.Email;
using RazorLight;

namespace Infrastructure.Services.Email
{
    public class RazorTemplateEngine : ITemplateEngine
    {
        private readonly RazorLightEngine _engine;

        public RazorTemplateEngine()
        {
            var templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Services",
                "Email",
                "Templates"
            );

            _engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatePath)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> RenderEmailTemplateAsync(string template, OtpEmailModel model)
        {
            return await _engine.CompileRenderAsync(template, model);
        }
    }
}
