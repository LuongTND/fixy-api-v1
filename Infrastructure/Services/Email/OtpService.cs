using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Application.Common.Models.Email;
using Application.Interfaces;
using Application.Interfaces.Services.Email;
using Domain.Entity.Identity;
using Domain.Enum;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Email
{
    public class OtpService : IOtpService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITemplateEngine _templateEngine;

        public OtpService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ITemplateEngine templateEngine
        )
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _templateEngine = templateEngine;
        }

        public async Task SendOtpAsync(string target, EmailPurpose purpose)
        {
            var oldOtps = await _unitOfWork
                .OtpVerifications.Where(x => x.Target == target && !x.IsUsed)
                .ToListAsync();

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            // 2. generate secure OTP
            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var otpEntity = new OtpVerification
            {
                Target = target,
                OtpCode = otpCode,
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsVerified = false,
                CreatedDate = DateTime.UtcNow,
            };

            await _unitOfWork.OtpVerifications.AddAsync(otpEntity);
            await _unitOfWork.SaveChangesAsync();
            if (IsEmail(target))
            {
                string html = "";
                switch (purpose)
                {
                    case EmailPurpose.Register:
                        html = await _templateEngine.RenderEmailTemplateAsync(
                            "RegisterOtpTemplate",
                            new OtpEmailModel { Otp = otpCode }
                        );
                        break;
                    case EmailPurpose.ForgotPassword:
                        //
                        html = await _templateEngine.RenderEmailTemplateAsync(
                            "ForgotPasswordOtpTemplate",
                            new OtpEmailModel { Otp = otpCode }
                        );
                        break;
                }
                await _emailService.SendEmailAsync(target, "Mã OTP của bạn", html);
            }
            else if (IsPhone(target))
            {
                //To do send SMS OTP
            }
        }

        public async Task VerifyOtpAsync(string target, string otpCode)
        {
            var otp = await _unitOfWork
                .OtpVerifications.OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(x => x.Target == target && x.OtpCode == otpCode && !x.IsUsed);

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
