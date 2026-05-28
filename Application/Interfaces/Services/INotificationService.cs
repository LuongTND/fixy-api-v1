using Application.Common;
using Application.DTOs.Notification;
using Domain.Enum;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(
            Guid userId,
            NotificationType type,
            string title,
            string body,
            string? deepLink = null,
            object? meta = null,
            string? code = null,
            CancellationToken cancellationToken = default);

        Task<OperationResult<PagedResponse<NotificationDto>>> GetNotificationsAsync(Guid userId,NotificationQuery query,CancellationToken cancellationToken = default);

        Task<OperationResult<int>> GetUnreadCountAsync(Guid userId,CancellationToken cancellationToken = default);

        Task<OperationResult> MarkAsReadAsync(Guid userId,Guid notificationId,CancellationToken cancellationToken = default);

        Task<OperationResult> MarkAllAsReadAsync(Guid userId,CancellationToken cancellationToken = default);

        Task<OperationResult<NotificationSettingsDto>> GetSettingsAsync(Guid userId,CancellationToken cancellationToken = default);

        Task<OperationResult<NotificationSettingsDto>> UpdateSettingsAsync(Guid userId,UpdateNotificationSettingsDto dto,CancellationToken cancellationToken = default);

        Task SendPromoNotificationToAllCustomersAsync(
            string title,
            string body,
            string? deepLink = null,
            object? meta = null,
            Guid? voucherId = null,
            Guid? campaignId = null,
            CancellationToken cancellationToken = default);
    }
}
