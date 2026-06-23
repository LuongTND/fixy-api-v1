using System.Text.Json;
using Application.Common;
using Application.DTOs.Notification;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Hubs;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IRepository<NotificationSetting> _settingRepository;
        private readonly IRepository<UserFcmToken> _fcmTokenRepository;
        private readonly IFcmService _fcmService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(
            INotificationRepository notificationRepository,
            IRepository<NotificationSetting> settingRepository,
            IRepository<UserFcmToken> fcmTokenRepository,
            IFcmService fcmService,
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger,
            IServiceProvider serviceProvider)
        {
            _notificationRepository = notificationRepository;
            _settingRepository = settingRepository;
            _fcmTokenRepository = fcmTokenRepository;
            _fcmService = fcmService;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task SendNotificationAsync(
            Guid userId,
            NotificationType type,
            string title,
            string body,
            string? deepLink = null,
            object? meta = null,
            string? code = null,
            CancellationToken cancellationToken = default)
        {
            // 1. Check user notification settings
            var setting = await _settingRepository.FirstOrDefaultAsync(
                s => s.UserId == userId, cancellationToken);

            bool isPushEnabled = true; // Mặc định bật nếu chưa có setting
            if (setting != null)
            {
                var shouldSkip = type switch
                {
                    NotificationType.Booking => !setting.NewBooking,
                    NotificationType.Payment => !setting.Payment,
                    NotificationType.Promo => !setting.Promotions,
                    _ => false
                };

                if (shouldSkip)
                {
                    _logger.LogInformation("Notification skipped for user {UserId}: type {Type} is disabled in settings.", userId, type);
                    return;
                }

                isPushEnabled = setting.ViaPush;
            }

            // 2. Serialize meta to JSON string
            string? metaJson = meta != null
                ? JsonSerializer.Serialize(meta, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                : null;

            // 3. Save notification to database
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Code = code,
                Title = title,
                Body = body,
                DeepLink = deepLink,
                Meta = metaJson,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Build DTO for SignalR broadcast
            var dto = new NotificationDto
            {
                Id = notification.Id,
                Type = type,
                Code = code,
                Title = title,
                Body = body,
                DeepLink = deepLink,
                Meta = meta,
                IsRead = false,
                CreatedDate = notification.CreatedDate
            };

            // 5. Push to web client via SignalR (In-App - khi user đang online)
            try
            {
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", dto, cancellationToken);

                _logger.LogInformation("SignalR notification sent to user {UserId}: {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to user {UserId}. Notification saved to DB.", userId);
            }

            // 6. Push via FCM (Web Push - khi user offline, trình duyệt chạy ngầm)
            if (isPushEnabled)
            {
                var userFcmTokens = await _fcmTokenRepository.FindAsync(
                    t => t.UserId == userId, cancellationToken);

                if (userFcmTokens.Count > 0)
                {
                    var tokenStrings = userFcmTokens.Select(t => t.Token).ToList();

                    // Fire-and-forget để không block response chính
                    _ = Task.Run(async () =>
                    {
                        await _fcmService.SendBatchPushNotificationAsync(
                            tokenStrings,
                            title,
                            body,
                            deepLink,
                            meta,
                            CancellationToken.None);
                    }, CancellationToken.None);
                }
            }
        }

        public async Task<OperationResult<PagedResponse<NotificationDto>>> GetNotificationsAsync(
            Guid userId,
            NotificationQuery query,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _notificationRepository.GetPagedByUserIdAsync(
                userId, query.Page, query.PageSize, query.IsRead, cancellationToken);

            var dtos = items.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Code = n.Code,
                Title = n.Title,
                Body = n.Body,
                DeepLink = n.DeepLink,
                Meta = string.IsNullOrEmpty(n.Meta) 
                    ? null 
                    : JsonSerializer.Deserialize<object>(n.Meta, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedDate = n.CreatedDate
            }).ToList();

            var response = new PagedResponse<NotificationDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.Page,
                PageSize = query.PageSize
            };

            return OperationResult<PagedResponse<NotificationDto>>.Success(response);
        }

        public async Task<OperationResult<int>> GetUnreadCountAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
            return OperationResult<int>.Success(count);
        }

        public async Task<OperationResult> MarkAsReadAsync(
            Guid userId,
            Guid notificationId,
            CancellationToken cancellationToken = default)
        {
            var notification = await _notificationRepository.FirstOrDefaultAsync(
                n => n.Id == notificationId && n.UserId == userId, cancellationToken);

            if (notification == null)
                return OperationResult.Failure("Notification not found");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Notification marked as read");
        }

        public async Task<OperationResult> MarkAllAsReadAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var unreadNotifications = await _notificationRepository.FindAsync(
                n => n.UserId == userId && !n.IsRead, cancellationToken);

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            _notificationRepository.UpdateRange(unreadNotifications);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success($"{unreadNotifications.Count} notifications marked as read");
        }

        public async Task<OperationResult<NotificationSettingsDto>> GetSettingsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(
                s => s.UserId == userId, cancellationToken);

            if (setting == null)
            {
                // Return default settings
                return OperationResult<NotificationSettingsDto>.Success(new NotificationSettingsDto
                {
                    NewBooking = true,
                    Payment = true,
                    StatusUpdate = true,
                    Promotions = true,
                    ViaPush = true,
                    ViaInApp = true
                });
            }

            var dto = new NotificationSettingsDto
            {
                NewBooking = setting.NewBooking,
                Payment = setting.Payment,
                StatusUpdate = setting.StatusUpdate,
                Promotions = setting.Promotions,
                ViaPush = setting.ViaPush,
                ViaSms = setting.ViaSms,
                ViaEmail = setting.ViaEmail,
                ViaInApp = setting.ViaInApp
            };

            return OperationResult<NotificationSettingsDto>.Success(dto);
        }

        public async Task<OperationResult<NotificationSettingsDto>> UpdateSettingsAsync(
            Guid userId,
            UpdateNotificationSettingsDto dto,
            CancellationToken cancellationToken = default)
        {
            var setting = await _settingRepository.FirstOrDefaultAsync(
                s => s.UserId == userId, cancellationToken);

            if (setting == null)
            {
                setting = new NotificationSetting { UserId = userId };
                await _settingRepository.AddAsync(setting, cancellationToken);
            }

            if (dto.NewBooking.HasValue) setting.NewBooking = dto.NewBooking.Value;
            if (dto.Payment.HasValue) setting.Payment = dto.Payment.Value;
            if (dto.StatusUpdate.HasValue) setting.StatusUpdate = dto.StatusUpdate.Value;
            if (dto.Promotions.HasValue) setting.Promotions = dto.Promotions.Value;
            if (dto.ViaPush.HasValue) setting.ViaPush = dto.ViaPush.Value;
            if (dto.ViaInApp.HasValue) setting.ViaInApp = dto.ViaInApp.Value;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetSettingsAsync(userId, cancellationToken);
        }

        public async Task SendPromoNotificationToAllCustomersAsync(
            string title,
            string body,
            string? deepLink = null,
            object? meta = null,
            Guid? voucherId = null,
            Guid? campaignId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var context = _serviceProvider.GetService(typeof(AppDbContext)) as AppDbContext
                    ?? throw new InvalidOperationException("AppDbContext not registered");

                // Eagerly load customers with their addresses
                var customers = await context.CustomerProfiles
                    .Include(c => c.Addresses)
                    .ToListAsync(cancellationToken);

                if (customers == null || !customers.Any())
                {
                    _logger.LogInformation("No customers found to send promotion notification.");
                    return;
                }

                // 1. If voucherId is provided, load the voucher and its restrictions
                Domain.Entity.Voucher? voucher = null;
                if (voucherId.HasValue)
                {
                    voucher = await context.Vouchers
                        .Include(v => v.Restrictions)
                        .FirstOrDefaultAsync(v => v.Id == voucherId.Value, cancellationToken);
                }

                // 2. If campaignId is provided, load the campaign and its vouchers with restrictions
                VoucherCampaign? campaign = null;
                if (campaignId.HasValue)
                {
                    campaign = await context.VoucherCampaigns
                        .Include(c => c.Vouchers)
                            .ThenInclude(v => v.Restrictions)
                        .FirstOrDefaultAsync(c => c.Id == campaignId.Value, cancellationToken);
                }

                // 3. Filter customers based on eligibility
                var eligibleCustomers = new List<CustomerProfile>();
                foreach (var customer in customers)
                {
                    bool isEligible = true;

                    if (voucher != null)
                    {
                        isEligible = await IsCustomerEligibleForVoucherAsync(customer, voucher, context, cancellationToken);
                    }
                    else if (campaign != null)
                    {
                        // A customer is eligible for a campaign if they are eligible for at least one voucher in it
                        if (campaign.Vouchers != null && campaign.Vouchers.Any())
                        {
                            isEligible = false;
                            foreach (var v in campaign.Vouchers)
                            {
                                if (await IsCustomerEligibleForVoucherAsync(customer, v, context, cancellationToken))
                                {
                                    isEligible = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isEligible)
                    {
                        eligibleCustomers.Add(customer);
                    }
                }

                if (!eligibleCustomers.Any())
                {
                    _logger.LogInformation("No eligible customers found for promotion notification: {Title}", title);
                    return;
                }

                _logger.LogInformation("Sending promotion notification to {Count} eligible customers out of {Total} total customers for: {Title}", 
                    eligibleCustomers.Count, customers.Count, title);

                foreach (var customer in eligibleCustomers)
                {
                    // SendNotificationAsync checks the customer's settings automatically
                    await SendNotificationAsync(
                        customer.UserId,
                        NotificationType.Promo,
                        title,
                        body,
                        deepLink,
                        meta,
                        code: null,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting promotion notification to eligible customers.");
            }
        }

        private async Task<bool> IsCustomerEligibleForVoucherAsync(
            CustomerProfile customer,
            Domain.Entity.Voucher voucher,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            if (voucher.Restrictions == null || !voucher.Restrictions.Any())
            {
                return true;
            }

            foreach (var restriction in voucher.Restrictions)
            {
                switch (restriction.Type)
                {
                    case RestrictionType.City:
                        if (!string.IsNullOrEmpty(restriction.Value))
                        {
                            var hasMatchingAddress = customer.Addresses != null && customer.Addresses.Any(a =>
                                !a.IsDeleted &&
                                !string.IsNullOrEmpty(a.City) &&
                                a.City.Contains(restriction.Value, StringComparison.OrdinalIgnoreCase));

                            // If a voucher is restricted to a city, we only notify customers who have a confirmed address in that city.
                            if (!hasMatchingAddress)
                            {
                                return false;
                            }
                        }
                        break;

                    case RestrictionType.IsFirstOrder:
                        var hasCompletedBooking = await context.Bookings.AnyAsync(
                            b => b.CustomerProfileId == customer.Id && b.Status == BookingStatus.Completed,
                            cancellationToken);

                        if (hasCompletedBooking)
                        {
                            return false;
                        }
                        break;
                }
            }

            return true;
        }

        public async Task<OperationResult> RegisterFcmTokenAsync(Guid userId, RegisterFcmTokenDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                // Kiểm tra token đã tồn tại chưa (chỉ 1 record duy nhất trên toàn hệ thống do unique index)
                var existingToken = await _fcmTokenRepository.FirstOrDefaultAsync(
                    t => t.Token == dto.Token, cancellationToken);

                if (existingToken != null)
                {
                    // Token đã tồn tại nhưng có thể thuộc về user khác (login tài khoản mới trên cùng browser)
                    if (existingToken.UserId != userId)
                    {
                        existingToken.UserId = userId;
                        _fcmTokenRepository.Update(existingToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    return OperationResult.Success("FCM token registered successfully.");
                }

                var fcmToken = new UserFcmToken
                {
                    UserId = userId,
                    Token = dto.Token,
                    DeviceType = dto.DeviceType ?? "Web",
                    Browser = dto.Browser
                };

                await _fcmTokenRepository.AddAsync(fcmToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("FCM token registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register FCM token for user {UserId}", userId);
                return OperationResult.Failure("An error occurred while registering the FCM token.");
            }
        }

        public async Task<OperationResult> UnregisterFcmTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var tokenEntity = await _fcmTokenRepository.FirstOrDefaultAsync(
                    t => t.Token == token && t.UserId == userId, cancellationToken);

                if (tokenEntity != null)
                {
                    _fcmTokenRepository.Remove(tokenEntity);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return OperationResult.Success("FCM token unregistered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unregister FCM token for user {UserId}", userId);
                return OperationResult.Failure("An error occurred while unregistering the FCM token.");
            }
        }
    }
}
