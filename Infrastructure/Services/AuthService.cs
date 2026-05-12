using System.Data;
using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Application.Settings;
using Domain.Entity;
using Google.Apis.Auth;
using Infrastructure.Helpers;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly GoogleSettings _googleSettings;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository userRepository,
            IUserOtpRepository userOtpRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IOptions<GoogleSettings> googleSettings,
            IOptions<JwtSettings> jwtSettings
        )
        {
            _userRepository = userRepository;
            _userOtpRepository = userOtpRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _googleSettings = googleSettings.Value;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<OperationResult<AuthResponseDto>> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken ct
        )
        {
            var otp = await _userOtpRepository.GetVerifiedOtpAsync(request.Target, ct);

            if (otp == null)
                return OperationResult<AuthResponseDto>.Failure("OTP is not verified");

            var existedUser = await _userRepository.GetByTargetAsync(request.Target, ct);

            if (existedUser != null)
                return OperationResult<AuthResponseDto>.Failure("Account already exists");

            var user = new User
            {
                FullName = request.FullName,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
            };

            if (IsPhone(request.Target))
            {
                user.Phone = request.Target;
                user.IsPhoneVerified = true;
            }
            else if (IsEmail(request.Target))
            {
                user.Email = request.Target;
                user.IsEmailVerified = true;
            }

            await _userRepository.AddAsync(user, ct);

            var role = await _roleRepository.GetCustomerRoleAsync(ct);

            await _userRoleRepository.AddAsync(new UserRole { User = user, RoleId = role.Id }, ct);

            var accessToken = _jwtService.GenerateAccessToken(user, new[] { role.Name });
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    User = user,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = new List<string> { role.Name },
                },
                "Register successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> LoginAsync(
            LoginRequestDto request,
            CancellationToken ct
        )
        {
            var user = await _userRepository.GetByTargetWithRoleAsync(request.Target, ct);

            if (user == null)
                return OperationResult<AuthResponseDto>.Failure("Invalid credentials");

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return OperationResult<AuthResponseDto>.Failure("Invalid credentials");

            var roles = user.UserRoles.Select(x => x.Role!.Name).ToList();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = roles,
                },
                "Login successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> GoogleLoginAsync(
            GoogleLoginRequestDto request,
            CancellationToken ct
        )
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                request.Credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.GoogleClientId },
                }
            );

            var user = await _userRepository.GetByTargetAsync(payload.Email, ct);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    IsEmailVerified = true,
                };

                await _userRepository.AddAsync(user, ct);
            }

            var role = await _roleRepository.GetCustomerRoleAsync(ct);

            var exists = await _userRoleRepository.ExistsAsync(
                ur => ur.UserId == user.Id && ur.RoleId == role.Id,
                ct
            );

            if (!exists)
            {
                await _userRoleRepository.AddAsync(
                    new UserRole { UserId = user.Id, RoleId = role.Id },
                    ct
                );
            }

            var accessToken = _jwtService.GenerateAccessToken(user, new[] { role.Name });
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = new List<string> { role.Name },
                },
                "Login successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> RefreshTokenAsync(
            string refreshToken,
            CancellationToken ct
        )
        {
            var tokenHash = TokenHasher.Hash(refreshToken);

            var token = await _refreshTokenRepository.GetValidTokenWithUserAsync(tokenHash, ct);

            if (token == null)
                return OperationResult<AuthResponseDto>.Failure("Invalid refresh token");

            if (token.ExpiresAt < DateTime.UtcNow)
                return OperationResult<AuthResponseDto>.Failure("Refresh token expired");

            var user = token.User;

            var roles = user!.UserRoles.Select(x => x.Role!.Name).ToList();

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(newRefreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = roles,
                },
                "Refresh successfully"
            );
        }

        public async Task<OperationResult> ChangePasswordAsync(
            ChangePasswordRequestDto request,
            CancellationToken ct
        )
        {
            var user = await _userRepository.GetByTargetAsync(request.Target, ct);

            if (user == null)
                return OperationResult.Failure("User not found");

            if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
                return OperationResult.Failure("Old password is not correct");

            if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
                return OperationResult.Failure("Match with old password");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult.Success("Change Password Success");
        }

        public async Task<OperationResult> ResetPasswordAsync(
            ResetPasswordRequestDto request,
            CancellationToken ct
        )
        {
            var otp = await _userOtpRepository.GetVerifiedOtpAsync(request.Target, ct);

            if (otp == null)
                return OperationResult.Failure("OTP is not verified");

            var user = await _userRepository.GetByTargetAsync(request.Target, ct);

            if (user == null)
                return OperationResult.Failure("User not found");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult.Success("Reset Password Success");
        }

        private bool IsEmail(string target)
        {
            return Regex.IsMatch(target, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsPhone(string target)
        {
            return Regex.IsMatch(target, @"^\d{9,11}$");
        }
    }
}
