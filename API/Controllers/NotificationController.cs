using Application.DTOs.Notification;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class NotificationController : ApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get paginated notification list for the current user.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] NotificationQuery query, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetNotificationsAsync(userId, query, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Get unread notification count.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Mark a single notification as read.
        /// </summary>
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _notificationService.MarkAsReadAsync(userId, id, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Mark all notifications as read.
        /// </summary>
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Get notification settings for the current user.
        /// </summary>
        [HttpGet("settings")]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetSettingsAsync(userId, cancellationToken);
            return HandleResult(result);
        }

        /// <summary>
        /// Update notification settings for the current user.
        /// </summary>
        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateNotificationSettingsDto dto, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var result = await _notificationService.UpdateSettingsAsync(userId, dto, cancellationToken);
            return HandleResult(result);
        }
    }
}
