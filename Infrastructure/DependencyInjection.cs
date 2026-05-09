using Application.Common.Interfaces;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Email;
using Application.Settings;
using Infrastructure.Common;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("FIXYDb"));
            }

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserOtpRepository, UserOtpRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();

            // Common Services
            services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

            // Options
            services.Configure<JwtSettings>(options =>
                configuration.GetSection("Jwt").Bind(options));
            services.Configure<OtpSettings>(options =>
                configuration.GetSection("Otp").Bind(options));
            services.Configure<SmtpSettings>(options =>
                configuration.GetSection("Smtp").Bind(options));

            // Auth Services
            services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}

