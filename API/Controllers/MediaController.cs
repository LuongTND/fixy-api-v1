using Application.DTOs.Media;
using Application.Interfaces.Services.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MediaController : ApiController
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadMediaFormDto request,CancellationToken cancellationToken)
        {
            var result = await _mediaService.UploadMediaAsync(request, cancellationToken);
            return HandleResult(result);
        }
    }
}
