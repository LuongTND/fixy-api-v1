using Application.Common;
using Application.DTOs.Chat;
using Application.Interfaces.Services.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/bookings/{bookingId:guid}/chat")]
    public class ChatController : ApiController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetHistory(Guid bookingId, [FromQuery] PagedQuery query, CancellationToken cancellationToken = default)
        {
            var result = await _chatService.GetChatHistoryAsync(bookingId, query, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(Guid bookingId, [FromForm] SendChatMessageRequest request, CancellationToken cancellationToken)
        {
            var result = await _chatService.SendMessageAsync(bookingId, request, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkRead(Guid bookingId, CancellationToken cancellationToken)
        {
            var result = await _chatService.MarkAsReadAsync(bookingId, cancellationToken);
            return HandleResult(result);
        }
    }
}
