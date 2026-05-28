using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
using Domain.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Booking
{
    public class BookingReminderSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingReminderSchedulerService> _logger;
        private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(5); // Scan every 5 minutes

        public BookingReminderSchedulerService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingReminderSchedulerService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingReminderSchedulerService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendBookingRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing booking reminder scheduler job.");
                }

                await Task.Delay(ScanInterval, stoppingToken);
            }

            _logger.LogInformation("BookingReminderSchedulerService stopped.");
        }

        private async Task SendBookingRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var serviceCategoryRepository = scope.ServiceProvider.GetRequiredService<IServiceCategoryRepository>();
            var customerProfileRepository = scope.ServiceProvider.GetRequiredService<ICustomerProfileRepository>();
            var workerProfileRepository = scope.ServiceProvider.GetRequiredService<IWorkerProfileRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // ScheduledAt is stored in UTC in the database
            var now = DateTime.UtcNow;
            var reminderWindowStart = now.AddMinutes(50);
            var reminderWindowEnd = now.AddMinutes(70); // 1 hour target window (+/- 10 minutes)

            _logger.LogDebug("Scanning for bookings scheduled between {Start} and {End} UTC", reminderWindowStart, reminderWindowEnd);

            // Find confirmed bookings in the 1 hour window
            var upcomingBookings = await bookingRepository.FindAsync(
                b => b.Status == BookingStatus.Confirmed
                     && b.ScheduledAt.HasValue
                     && b.ScheduledAt.Value >= reminderWindowStart
                     && b.ScheduledAt.Value <= reminderWindowEnd
                     && b.IsDeleted == false,
                cancellationToken
            );

            if (!upcomingBookings.Any())
            {
                return;
            }

            _logger.LogInformation("Found {Count} upcoming confirmed bookings for 1-hour reminders.", upcomingBookings.Count);

            foreach (var booking in upcomingBookings)
            {
                try
                {
                    var reminderCode = $"REMINDER_1H_{booking.Id}";

                    // Load category name
                    var category = await serviceCategoryRepository.GetByIdAsync(booking.CategoryId, cancellationToken);
                    var serviceName = category?.Name ?? "Dịch vụ";
                    
                    // Convert UTC ScheduledAt to Vietnamese local time (GMT+7) for notification text
                    var localTime = booking.ScheduledAt!.Value.AddHours(7);
                    var timeStr = localTime.ToString("HH:mm");

                    // 1. Notify Customer
                    var customerProfile = await customerProfileRepository.GetByIdAsync(booking.CustomerProfileId, cancellationToken);
                    if (customerProfile != null)
                    {
                        var customerReminderCode = $"{reminderCode}_customer";
                        var hasSentToCustomer = await notificationRepository.ExistsAsync(
                            n => n.UserId == customerProfile.UserId && n.Code == customerReminderCode,
                            cancellationToken
                        );

                        if (!hasSentToCustomer)
                        {
                            var meta = new
                            {
                                bookingId = booking.Id,
                                scheduledTime = booking.ScheduledAt.Value,
                                serviceName
                            };

                            var title = "Nhắc nhở lịch hẹn dịch vụ";
                            var body = $"Lịch hẹn dịch vụ {serviceName} của bạn sẽ bắt đầu vào lúc {timeStr} (khoảng 1 giờ nữa). Vui lòng chuẩn bị đón thợ.";
                            var deepLink = $"/customer/bookings/{booking.Id}";

                            await notificationService.SendNotificationAsync(
                                customerProfile.UserId,
                                NotificationType.Booking,
                                title,
                                body,
                                deepLink,
                                meta,
                                customerReminderCode,
                                cancellationToken
                            );

                            _logger.LogInformation("Sent 1h reminder to customer {UserId} for booking {BookingId}", customerProfile.UserId, booking.Id);
                        }
                    }

                    // 2. Notify Worker if assigned
                    if (booking.WorkerProfileId.HasValue)
                    {
                        var workerProfile = await workerProfileRepository.GetByIdAsync(booking.WorkerProfileId.Value, cancellationToken);
                        if (workerProfile != null)
                        {
                            var workerReminderCode = $"{reminderCode}_worker";
                            var hasSentToWorker = await notificationRepository.ExistsAsync(
                                n => n.UserId == workerProfile.UserId && n.Code == workerReminderCode,
                                cancellationToken
                            );

                            if (!hasSentToWorker)
                            {
                                var meta = new
                                {
                                    bookingId = booking.Id,
                                    scheduledTime = booking.ScheduledAt.Value,
                                    serviceName
                                };

                                var title = "Nhắc nhở lịch hẹn công việc";
                                var body = $"Bạn có lịch hẹn sửa chữa dịch vụ {serviceName} lúc {timeStr} (khoảng 1 giờ nữa). Vui lòng chuẩn bị di chuyển đến địa chỉ khách hàng.";
                                var deepLink = $"/worker/bookings/{booking.Id}";

                                await notificationService.SendNotificationAsync(
                                    workerProfile.UserId,
                                    NotificationType.Booking,
                                    title,
                                    body,
                                    deepLink,
                                    meta,
                                    workerReminderCode,
                                    cancellationToken
                                );

                                _logger.LogInformation("Sent 1h reminder to worker {UserId} for booking {BookingId}", workerProfile.UserId, booking.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminder for booking {BookingId}", booking.Id);
                }
            }
        }
    }
}
