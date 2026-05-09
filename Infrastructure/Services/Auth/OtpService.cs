using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Application.Settings;
using Domain.Entity;
using Domain.Enum;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth
{
    public class OtpService : IOtpService
    {
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly OtpSettings _settings;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IUserOtpRepository userOtpRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            IOptions<OtpSettings> settings,
            ILogger<OtpService> logger)
        {
            _userOtpRepository = userOtpRepository ?? throw new ArgumentNullException(nameof(userOtpRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _settings = (settings ?? throw new ArgumentNullException(nameof(settings))).Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> CreateOtpAsync(Guid userId, UserOtpType type, string? ipAddress, CancellationToken cancellationToken = default)
        {
            var activeOtps = await _userOtpRepository.GetActiveOtpsAsync(userId, type, cancellationToken);

            foreach (var otp in activeOtps)
            {
                otp.IsUsed = true;
                otp.UsedAt = _dateTimeProvider.UtcNow;
            }

            var code = GenerateCode();
            var newOtp = new UserOtp
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                OtpHash = HashOtp(code),
                AttemptCount = 0,
                ExpiresAt = _dateTimeProvider.UtcNow.AddMinutes(_settings.ExpiresMinutes),
                IsUsed = false,
                IpAddress = ipAddress
            };

            await _userOtpRepository.AddAsync(newOtp, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return code;
        }

        public async Task<bool> VerifyOtpAsync(Guid userId, UserOtpType type, string otp, CancellationToken cancellationToken = default)
        {
            var activeOtp = await _userOtpRepository.GetLatestActiveOtpAsync(userId, type, cancellationToken);
            if (activeOtp == null)
            {
                return false;
            }

            var now = _dateTimeProvider.UtcNow;
            if (activeOtp.ExpiresAt <= now)
            {
                return false;
            }

            if (activeOtp.LockedUntil.HasValue && activeOtp.LockedUntil.Value > now)
            {
                _logger.LogWarning("OTP verification blocked due to lockout. UserId={UserId} Type={OtpType}", userId, type);
                return false;
            }

            if (!SlowEquals(activeOtp.OtpHash, HashOtp(otp)))
            {
                activeOtp.AttemptCount += 1;
                if (activeOtp.AttemptCount >= _settings.MaxAttempts)
                {
                    activeOtp.LockedUntil = activeOtp.ExpiresAt;
                    _logger.LogWarning("OTP locked due to max attempts. UserId={UserId} Type={OtpType}", userId, type);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return false;
            }

            activeOtp.IsUsed = true;
            activeOtp.UsedAt = now;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static string GenerateCode()
        {
            var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return number.ToString("D6");
        }

        private string HashOtp(string otp)
        {
            if (string.IsNullOrWhiteSpace(_settings.Secret))
            {
                throw new InvalidOperationException("OTP secret is not configured");
            }

            var key = Encoding.UTF8.GetBytes(_settings.Secret);
            using var hmac = new HMACSHA256(key);
            var bytes = Encoding.UTF8.GetBytes(otp);
            var hash = hmac.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private static bool SlowEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left);
            var rightBytes = Encoding.UTF8.GetBytes(right);
            return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }
    }
}
