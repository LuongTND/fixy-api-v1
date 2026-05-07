using Application.Interfaces;
using Application.Interfaces.Services.Auth;
using Domain.Entity.Identity;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Auth
{
    public class OtpService : IOtpService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OtpService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendOtpAsync(string target, OtpType type)
        {
            var oldOtps = _unitOfWork
                .OtpVerifications.Where(x => x.Target == target && x.Type == type && !x.IsUsed)
                .ToList();

            foreach (var oldOtp in oldOtps)
            {
                oldOtp.IsUsed = true;
            }

            var otpCode = new Random().Next(100000, 999999).ToString();

            var otp = new OtpVerification
            {
                Target = target,
                Type = type,
                OtpCode = otpCode,
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsVerified = false,
            };

            await _unitOfWork.OtpVerifications.AddAsync(otp);

            await _unitOfWork.SaveChangesAsync();

            switch (type)
            {
                case OtpType.Sms:
                    Console.WriteLine($"SMS OTP for {target}: {otpCode}");
                    break;

                case OtpType.Email:
                    Console.WriteLine($"EMAIL OTP for {target}: {otpCode}");
                    break;
            }
        }

        public async Task VerifyOtpAsync(string target, OtpType type, string otpCode)
        {
            var otp = await _unitOfWork
                .OtpVerifications.OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x =>
                    x.Target == target && x.Type == type && x.OtpCode == otpCode && !x.IsUsed
                );

            if (otp is null)
            {
                throw new BusinessException("Invalid OTP");
            }

            if (otp.ExpiryDate < DateTime.UtcNow)
            {
                throw new BusinessException("OTP expired");
            }

            otp.IsUsed = true;
            otp.IsVerified = true;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
