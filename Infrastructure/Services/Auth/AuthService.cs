using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Services.Auth;
using Domain.Entity.Identity;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPasswordHasher _passwordHasher;

        private readonly IJwtService _jwtService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtService jwtService
        )
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var verifiedOtp = await _unitOfWork
                .OtpVerifications.OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x => x.Target == request.Target && x.IsVerified);

            if (verifiedOtp is null)
            {
                throw new BusinessException("OTP is not verified");
            }

            var existedUser = await _unitOfWork.Users.FirstOrDefaultAsync(x =>
                x.PhoneNumber == request.Target || x.Email == request.Target
            );

            if (existedUser is not null)
            {
                throw new BusinessException("Account already exists");
            }

            var user = new User
            {
                FullName = request.FullName,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                Gender = Gender.Other,
                Status = UserStatus.Active,
            };

            if (IsPhone(request.Target))
            {
                user.PhoneNumber = request.Target;
                user.IsEmailVerified = true;
            }
            else if (IsEmail(request.Target))
            {
                user.Email = request.Target;
                user.IsPhoneVerified = true;
            }

            await _unitOfWork.Users.AddAsync(user);

            await _unitOfWork.SaveChangesAsync();

            var customerRole = await _unitOfWork.Roles.FirstAsync(x => x.Code == "CUSTOMER");

            var userRole = new UserRole { UserId = user.Id, RoleId = customerRole.Id };

            await _unitOfWork.UserRoles.AddAsync(userRole);

            var accessToken = _jwtService.GenerateAccessToken(
                user,
                new List<string> { customerRole.Code }
            );

            var refreshToken = _jwtService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                }
            );

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _unitOfWork
                .Users.Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x =>
                    x.PhoneNumber == request.Target || x.Email == request.Target
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

            var roles = user.UserRoles.Select(x => x.Role.Code).ToList();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);

            var refreshToken = _jwtService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                }
            );

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var existedRefreshToken = await _unitOfWork
                .RefreshTokens.Include(x => x.User)
                    .ThenInclude(x => x.UserRoles)
                        .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Token == refreshToken);

            if (existedRefreshToken is null)
            {
                throw new BusinessException("Invalid refresh token");
            }

            if (existedRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                throw new BusinessException("Refresh token expired");
            }

            var user = existedRefreshToken.User;

            var roles = user.UserRoles.Select(x => x.Role.Code).ToList();

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);

            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // revoke old token
            _unitOfWork.RefreshTokens.Remove(existedRefreshToken);

            // add new token
            await _unitOfWork.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                }
            );

            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            };
        }

        public async Task ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(x =>
                x.Email == request.Target || x.PhoneNumber == request.Target
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

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var otp = await _unitOfWork.OtpVerifications.FirstOrDefaultAsync(x =>
                x.Target == request.Target && x.OtpCode == request.Otp && !x.IsVerified
            );

            if (otp is null)
            {
                throw new BusinessException("Invalid OTP");
            }

            if (otp.ExpiryDate < DateTime.UtcNow)
            {
                throw new BusinessException("OTP expired");
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(x =>
                x.Email == request.Target || x.PhoneNumber == request.Target
            );

            if (user is null)
            {
                throw new NotFoundException("User not found");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            otp.IsVerified = true;

            // revoke OTP sau khi dùng
            _unitOfWork.OtpVerifications.Remove(otp);

            await _unitOfWork.SaveChangesAsync();
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
