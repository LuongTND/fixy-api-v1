using System.Security.Cryptography;
using System.Text;
using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Email;
using Application.Settings;
using Domain.Entity;
using Domain.Enum;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IUserSessionRepository userSessionRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            ICustomerProfileRepository customerProfileRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IOtpService otpService,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IDateTimeProvider dateTimeProvider,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userSessionRepository = userSessionRepository ?? throw new ArgumentNullException(nameof(userSessionRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
            _customerProfileRepository = customerProfileRepository ?? throw new ArgumentNullException(nameof(customerProfileRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _jwtSettings = (jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings))).Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OperationResult> SignupAsync(SignupDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var email = NormalizeEmail(request.Email);

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Signup failed due to missing email or password. Ip={IpAddress}", ipAddress);
                return OperationResult.Failure("Email and password are required");
            }

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user != null)
            {
                if (user.IsActive)
                {
                    _logger.LogWarning("Signup failed because email already exists. Email={Email} Ip={IpAddress}", email, ipAddress);
                    return OperationResult.Failure("Email already exists");
                }
            }
            else
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Phone = request.Phone,
                    PasswordHash = _passwordHasher.HashPassword(request.Password),
                    IsActive = false
                };

                await _userRepository.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var otp = await _otpService.CreateOtpAsync(
                user.Id,
                UserOtpType.EmailVerify,
                ipAddress,
                cancellationToken);

            await _emailService.SendOtpEmailAsync(
                user.Email ?? email,
                otp,
                cancellationToken);

            _logger.LogInformation("Signup OTP sent. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);

            return OperationResult.Success("OTP sent");
        }

        public async Task<OperationResult<AuthResponseDto>> VerifySignupOtpAsync(VerifySignupOtpDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Verify signup OTP failed: user not found. Email={Email} Ip={IpAddress}", email, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("OTP is invalid");
            }

            if (user.IsActive)
            {
                _logger.LogWarning("Verify signup OTP failed: user already active. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("User already active");
            }

            var verified = await _otpService.VerifyOtpAsync(user.Id, UserOtpType.EmailVerify, request.Otp, cancellationToken);
            if (!verified)
            {
                _logger.LogWarning("Verify signup OTP failed: invalid OTP. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("OTP is invalid");
            }

            user.IsActive = true;

            var role = await _roleRepository.GetByNameAsync("Customer", cancellationToken);
            if (role == null)
            {
                _logger.LogError("Default role not found during verify signup. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("Default role not found");
            }

            var hasRole = await _userRoleRepository.ExistsAsync(user.Id, role.Id, cancellationToken);
            if (!hasRole)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    AssignedAt = _dateTimeProvider.UtcNow
                };

                await _userRoleRepository.AddAsync(userRole, cancellationToken);
            }

            var hasProfile = await _customerProfileRepository.ExistsAsync(x => x.UserId == user.Id, cancellationToken);
            if (!hasProfile)
            {
                var customerProfile = new CustomerProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = user.Email ?? string.Empty
                };

                await _customerProfileRepository.AddAsync(customerProfile, cancellationToken);
            }

            var session = CreateSession(user.Id, ipAddress, null, null, null, null, null);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = CreateRefreshToken(user.Id, session.Id, refreshTokenValue);

            await _userSessionRepository.AddAsync(session, cancellationToken);
            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Signup verified and session created. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);

            return OperationResult<AuthResponseDto>.Success(BuildAuthResponse(user, refreshTokenValue));
        }

        public async Task<OperationResult<AuthResponseDto>> LoginAsync(LoginDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
            {
                _logger.LogWarning("Login failed: invalid credentials. Email={Email} Ip={IpAddress}", email, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("Email or password is invalid");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: user inactive. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("User is inactive");
            }

            var session = CreateSession(user.Id, ipAddress, request.DeviceId, request.DeviceName, request.DeviceType, request.Os, request.AppVersion);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();
            var refreshToken = CreateRefreshToken(user.Id, session.Id, refreshTokenValue);

            await _userSessionRepository.AddAsync(session, cancellationToken);
            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login successful. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);

            return OperationResult<AuthResponseDto>.Success(BuildAuthResponse(user, refreshTokenValue));
        }

        public async Task<OperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var tokenHash = HashToken(request.RefreshToken);
            var refreshToken = await _refreshTokenRepository.GetByTokenHashWithUserSessionAsync(tokenHash, cancellationToken);

            if (refreshToken == null || refreshToken.User == null)
            {
                _logger.LogWarning("Refresh failed: token invalid. Ip={IpAddress}", ipAddress);
                return OperationResult<AuthResponseDto>.Failure("Refresh token is invalid");
            }

            var now = _dateTimeProvider.UtcNow;
            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning("Refresh failed: token revoked. UserId={UserId} Ip={IpAddress}", refreshToken.UserId, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("Refresh token is revoked");
            }

            if (refreshToken.ExpiresAt <= now)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = now;
                refreshToken.RevokedReason = "Expired";
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Refresh failed: token expired. UserId={UserId} Ip={IpAddress}", refreshToken.UserId, ipAddress);
                return OperationResult<AuthResponseDto>.Failure("Refresh token is expired");
            }

            var newRefreshValue = _tokenService.GenerateRefreshToken();
            var newRefreshToken = CreateRefreshToken(refreshToken.UserId, refreshToken.SessionId, newRefreshValue);

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = now;
            refreshToken.RevokedReason = "Replaced";
            refreshToken.ReplacedById = newRefreshToken.Id;

            if (refreshToken.Session != null)
            {
                refreshToken.Session.LastActiveAt = now;
                refreshToken.Session.IpAddress = ipAddress;
            }

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Refresh successful. UserId={UserId} Ip={IpAddress}", refreshToken.UserId, ipAddress);

            return OperationResult<AuthResponseDto>.Success(BuildAuthResponse(refreshToken.User, newRefreshValue));
        }

        public async Task<OperationResult> LogoutAsync(LogoutDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var tokenHash = HashToken(request.RefreshToken);
            var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
            if (refreshToken == null)
            {
                _logger.LogInformation("Logout completed: token not found. Ip={IpAddress}", ipAddress);
                return OperationResult.Success("Logout completed");
            }

            if (!refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = _dateTimeProvider.UtcNow;
                refreshToken.RevokedReason = "Logout";
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Logout completed. UserId={UserId} Ip={IpAddress}", refreshToken.UserId, ipAddress);

            return OperationResult.Success("Logout completed");
        }

        public async Task<OperationResult> RequestPasswordOtpAsync(RequestPasswordOtpDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogInformation("Password OTP requested for non-existing email. Email={Email} Ip={IpAddress}", email, ipAddress);
                return OperationResult.Success("OTP sent if email exists");
            }

            var otp = await _otpService.CreateOtpAsync(user.Id, UserOtpType.ResetPassword, ipAddress, cancellationToken);
            await _emailService.SendOtpEmailAsync(user.Email ?? email, otp, cancellationToken);

            _logger.LogInformation("Password OTP sent. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);

            return OperationResult.Success("OTP sent if email exists");
        }

        public async Task<OperationResult> ChangePasswordByOtpAsync(ChangePasswordByOtpDto request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Change password OTP failed: user not found. Email={Email} Ip={IpAddress}", email, ipAddress);
                return OperationResult.Failure("OTP is invalid");
            }

            var verified = await _otpService.VerifyOtpAsync(user.Id, UserOtpType.ResetPassword, request.Otp, cancellationToken);
            if (!verified)
            {
                _logger.LogWarning("Change password OTP failed: invalid OTP. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);
                return OperationResult.Failure("OTP is invalid");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            var now = _dateTimeProvider.UtcNow;
            var activeTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(user.Id, cancellationToken);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = now;
                token.RevokedReason = "Password changed";
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Password changed by OTP. UserId={UserId} Ip={IpAddress}", user.Id, ipAddress);
            return OperationResult.Success("Password updated");
        }

        private AuthResponseDto BuildAuthResponse(User user, string refreshTokenValue)
        {
            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                AccessToken = _tokenService.GenerateAccessToken(user),
                RefreshToken = refreshTokenValue,
                ExpiresAt = _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes)
            };
        }

        private UserSession CreateSession(Guid userId, string ipAddress, string? deviceId, string? deviceName, SessionDeviceType? deviceType, string? os, string? appVersion)
        {
            return new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DeviceId = deviceId,
                DeviceName = deviceName,
                DeviceType = deviceType,
                Os = os,
                AppVersion = appVersion,
                IpAddress = ipAddress,
                LastActiveAt = _dateTimeProvider.UtcNow,
                ExpiresAt = _dateTimeProvider.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
                IsActive = true
            };
        }

        private RefreshToken CreateRefreshToken(Guid userId, Guid sessionId, string refreshTokenValue)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionId = sessionId,
                TokenHash = HashToken(refreshTokenValue),
                ExpiresAt = _dateTimeProvider.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
                IsRevoked = false
            };
        }

        private static string NormalizeEmail(string? email)
        {
            return (email ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
