using System.Text;
using Application.Common.Interfaces;
using Application.Common.Settings;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Booking;
using Application.Interfaces.Services.Email;
using Application.Interfaces.Services.Media;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.Services.ServiceCategory;
using Application.Service;
using Application.Settings;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Email;
using Infrastructure.Services.Medias;
using Infrastructure.Services.Payment;
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

            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            // SMTP Settings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            // Blob Settings
            services.Configure<BlobSettings>(configuration.GetSection("BlobSettings"));
            // VNPay Settings
            services.Configure<VNPaySettings>(configuration.GetSection("VnPaySettings"));
            services.Configure<MoMoSettings>(configuration.GetSection("MoMoSettings"));
            // Cloudinary Settings
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            // Blob Settings
            services.Configure<BlobSettings>(configuration.GetSection("BlobSettings"));

            services.AddSingleton<IEmailQueue, EmailQueue>();
            services.AddHostedService<EmailBackgroundService>();
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

            // Services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddSingleton<ITemplateEngine, RazorTemplateEngine>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWorkerProfileService, WorkerProfileService>();
            services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IBlobService, BlobService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserOtpRepository, UserOtpRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IWorkerProfileRepository, WorkerProfileRepository>();
            services.AddScoped<IWorkerCertificateRepository, WorkerCertificateRepository>();
            services.AddScoped<IWorkerServiceRepository, WorkerServiceRepository>();
            services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
            services.AddScoped<IWalletRepository, WalletRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

            services.AddScoped<IWorkerServiceRepository, WorkerServiceRepository>();
            services.AddScoped<IMediaRepository, MediaRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
            services.AddScoped<MoMoService>();
            services.AddScoped<VnPayService>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
            services.AddScoped<IPaymentService, PaymentService>();
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
