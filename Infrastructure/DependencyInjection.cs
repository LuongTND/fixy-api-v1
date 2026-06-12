using System.Text;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Interfaces;
using Application.Interfaces.Hubs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Booking;
using Application.Interfaces.Services.Chat;
using Application.Interfaces.Services.Email;
using Application.Interfaces.Services.Media;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.Services.ServiceCategory;
using Application.Interfaces.Services.Voucher;
using Application.Interfaces.Services.Worker;
using Application.Services;
using Application.Settings;
using Domain.Entity;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Booking;
using Infrastructure.Services.Chat;
using Infrastructure.Services.Email;
using Infrastructure.Services.Medias;
using Infrastructure.Services.Notifications;
using Infrastructure.Services.Payment;
using Infrastructure.Services.ServiceCategories;
using Infrastructure.Services.Support;
using Infrastructure.Services.Vouchers;
using Infrastructure.Services.Worker;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString)
                );
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("FIXYDb")
                );
            }

            // Jwt Settings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<GoogleSettings>(configuration.GetSection("GoogleSettings"));

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            // SMTP Settings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            // WorkerMatching Settings
            services.Configure<WorkerMatchingSettings>(
                configuration.GetSection("WorkerMatchingSettings")
            );
            // Blob Settings
            services.Configure<BlobSettings>(configuration.GetSection("BlobSettings"));
            // VNPay Settings
            services.Configure<VNPaySettings>(configuration.GetSection("VnPaySettings"));
            services.Configure<MoMoSettings>(configuration.GetSection("MoMoSettings"));
            services.Configure<PayOSSettings>(configuration.GetSection("PayOSSetings"));

            // Cloudinary Settings
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            // Blob Settings
            services.Configure<BlobSettings>(configuration.GetSection("BlobSettings"));

            services.AddSingleton<IEmailQueue, EmailQueue>();
            services.AddHostedService<EmailBackgroundService>();
            services.AddHostedService<BookingTimeoutBackgroundService>();
            services.AddHostedService<VoucherCampaignSchedulerService>();
            services.AddHostedService<BookingReminderSchedulerService>();
            // Authentication
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings!.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Secret)
                        ),
                    };
                });

            services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
            services.AddScoped<IWorkerProfileService, WorkerProfileService>();
            services.AddScoped<IWorkerWeeklyScheduleService, WorkerWeeklyScheduleService>();
            services.AddScoped<IWorkerScheduleExceptionService, WorkerScheduleExceptionService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserOtpRepository, UserOtpRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IWorkerProfileRepository, WorkerProfileRepository>();
            services.AddScoped<IWorkerCertificateRepository, WorkerCertificateRepository>();
            services.AddScoped<IWorkerServiceRepository, WorkerServiceRepository>();
            services.AddScoped<IWorkerWeeklyScheduleRepository, WorkerWeeklyScheduleRepository>();
            services.AddScoped<
                IWorkerScheduleExceptionRepository,
                WorkerScheduleExceptionRepository
            >();
            services.AddScoped<IWorkerPayoutAccountRepository, WorkerPayoutAccountRepository>();
            services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IWorkerServiceRepository, WorkerServiceRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
            services.AddScoped<IPayoutRequestRepository, PayoutRequestRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            services.AddScoped<IWorkerMatchingQueueRepository, WorkerMatchingQueueRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<IVoucherCampaignRepository, VoucherCampaignRepository>();
            services.AddScoped<IVoucherUsageHistoryRepository, VoucherUsageHistoryRepository>();
            services.AddScoped<IBookingVoucherRepository, BookingVoucherRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped(
                typeof(IRepository<NotificationSetting>),
                typeof(Repository<NotificationSetting>)
            );
            services.AddScoped(typeof(IRepository<UserFcmToken>), typeof(Repository<UserFcmToken>));
            // Firebase Cloud Messaging - Singleton vì FirebaseApp là global singleton
            services.AddSingleton<IFcmService, FcmService>();
            // Services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddSingleton<ITemplateEngine, RazorTemplateEngine>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<IEnumService, EnumService>();
            services.AddScoped<IBookingHubService, BookingHubService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IWorkerLocationService, WorkerLocationService>();
            services.AddScoped<IBookingDraftService, BookingDraftService>();
            services.AddScoped<IWorkerMatchingService, WorkerMatchingService>();
            services.AddScoped<IWorkerPayoutAccountService, WorkerPayoutAccountService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IReportExportService, ReportExportService>();
            services.AddScoped<IPayoutService, PayoutService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<MoMoService>();
            services.AddScoped<VnPayService>();
            services.AddScoped<PayOSService>();

            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IVoucherCampaignService, VoucherCampaignService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ISupportTicketService, SupportTicketService>();
            services.AddScoped<ISupportHubService, SupportHubService>();

            // Unit Of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Common Services
            services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

            var redisSettings = configuration.GetSection("RedisSettings").Get<RedisSettings>();
            if (!string.IsNullOrWhiteSpace(redisSettings?.ConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisSettings.ConnectionString;
                    options.InstanceName = redisSettings.InstanceName;
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            return services;
        }
    }
}
