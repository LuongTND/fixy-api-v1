using System.Text.RegularExpressions;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Services.Auth;
using Application.Settings;
using Domain.Entity;
using Domain.Exceptions;
using Google.Apis.Auth;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IJwtService _jwtService;
        private readonly GoogleSettings _googleSettings;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IOptions<GoogleSettings> googleSettings,
            IOptions<JwtSettings> jwtSettings
        )
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _googleSettings = googleSettings.Value;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var verifiedOtp = await _unitOfWork
                .Otps.OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(
                    x =>
                        x.Target == request.Target
                        && x.ExpiryDate > DateTime.UtcNow
                        && x.IsVerified,
                    cancellationToken
                );

            if (verifiedOtp is null)
            {
                throw new BusinessException("OTP is not verified");
            }

            var existedUser = await _unitOfWork.Users.FirstOrDefaultAsync(
                x => x.Phone == request.Target || x.Email == request.Target,
                cancellationToken
            );

            if (existedUser is not null)
            {
                throw new BusinessException("Account already exists");
            }

            var user = new User
            {
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                FullName = request.FullName,
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

            await _unitOfWork.Users.AddAsync(user, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var customerRole = await _unitOfWork.Roles.FirstAsync(
                x => x.Name == "CUSTOMER",
                cancellationToken
            );

            var userRole = new UserRole { UserId = user.Id, RoleId = customerRole.Id };

            await _unitOfWork.UserRoles.AddAsync(userRole, cancellationToken);

            var accessToken = _jwtService.GenerateAccessToken(
                user,
                new List<string> { customerRole.Name }
            );

            var refreshToken = _jwtService.GenerateRefreshToken();
            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<AuthResponseDto> LoginAsync(
            LoginRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var user = await _unitOfWork
                .Users.Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(
                    x => x.Phone == request.Target || x.Email == request.Target,
                    cancellationToken
                );

            if (user is null)
            {
                throw new BusinessException("Invalid credentials");
            }

            var isValidPassword = _passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash
            );

            if (!isValidPassword)
            {
                throw new BusinessException("Invalid credentials");
            }

            var roles = user.UserRoles.Select(x => x.Role?.Name).ToList();

            var accessToken = _jwtService.GenerateAccessToken(user, roles!);

            var refreshToken = _jwtService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(
            GoogleLoginRequestDto requestDto,
            CancellationToken cancellationToken
        )
        {
            // Xác thực token Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                requestDto.Credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.GoogleClientId },
                }
            );

            // Tìm user theo email
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(
                u => u.Email == payload.Email,
                cancellationToken
            );
            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    IsEmailVerified = true,
                    FullName = payload.Name,
                };
                await _unitOfWork.Users.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            var customerRole = await _unitOfWork.Roles.FirstAsync(x => x.Name == "CUSTOMER");

            var userRole = new UserRole { UserId = user.Id, RoleId = customerRole.Id };

            var hasRole = await _unitOfWork.UserRoles.AnyAsync(
                x => x.UserId == user.Id && x.RoleId == customerRole.Id,
                cancellationToken
            );

            if (!hasRole)
            {
                await _unitOfWork.UserRoles.AddAsync(
                    new UserRole { UserId = user.Id, RoleId = customerRole.Id },
                    cancellationToken
                );
            }
            var accessToken = _jwtService.GenerateAccessToken(
                user,
                new List<string> { customerRole.Name }
            );

            var refreshToken = _jwtService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                }
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            var existedRefreshToken = await _unitOfWork
                .RefreshTokens.Include(x => x.User!)
                    .ThenInclude(x => x.UserRoles)
                        .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(
                    x => x.TokenHash == TokenHasher.Hash(refreshToken) && !x.IsRevoked,
                    cancellationToken
                );

            if (existedRefreshToken is null)
            {
                throw new BusinessException("Invalid refresh token");
            }

            if (existedRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new BusinessException("Refresh token expired");
            }

            var user = existedRefreshToken.User ?? throw new BusinessException("User not found");

            var roles = user
                .UserRoles.Select(x => x.Role?.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);

            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // revoke old token
            existedRefreshToken.IsRevoked = true;
            existedRefreshToken.RevokedAt = DateTime.UtcNow;

            // add new token
            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(newRefreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                cancellationToken
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }

        public async Task ChangePasswordAsync(
            ChangePasswordRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(
                x => x.Email == request.Target || x.Phone == request.Target,
                cancellationToken
            );

            if (user is null)
            {
                throw new NotFoundException("User not found");
            }
            if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                throw new BusinessException("Old password is not correct");
            }
            if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
            {
                throw new BusinessException("Match with old password");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetPasswordAsync(
            ResetPasswordRequestDto request,
            CancellationToken cancellationToken
        )
        {
            var verifiedOtp = await _unitOfWork
                .Otps.OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(
                    x =>
                        x.Target == request.Target
                        && x.ExpiryDate > DateTime.UtcNow
                        && x.IsVerified,
                    cancellationToken
                );

            if (verifiedOtp is null)
            {
                throw new BusinessException("OTP is not verified");
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(
                x => x.Email == request.Target || x.Phone == request.Target,
                cancellationToken
            );

            if (user is null)
            {
                throw new NotFoundException("User not found");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
