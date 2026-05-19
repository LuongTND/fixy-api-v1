using Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Chat
{
    public class SendChatMessageRequest
    {
        public ChatMessageType Type { get; set; } = ChatMessageType.Text;
        public string? Content { get; set; }
        public IFormFile? File { get; set; }
    }
}
