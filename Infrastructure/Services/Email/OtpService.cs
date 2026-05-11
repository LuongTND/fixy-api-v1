using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Application.Common;
using Application.Common.Models.Email;
using Application.Interfaces;
using Application.Interfaces.Services.Email;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services.Email
{
    public class OtpService : IOtpService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailQueue _emailQueue;
        private readonly ITemplateEngine _templateEngine;

        public OtpService(
            IUnitOfWork unitOfWork,
            IEmailQueue emailQueue,
            ITemplateEngine templateEngine
        )
        {
            _unitOfWork = unitOfWork;
            _emailQueue = emailQueue;
            _templateEngine = templateEngine;
        }

        public async Task<OperationResult> SendOtpAsync(string target, EmailPurpose purpose)
        {
            var oldOtps = await _unitOfWork.Otps.GetUnusedOtpsAsync(target);

            foreach (var otp in oldOtps)
                otp.IsUsed = true;

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var entity = new UserOtp
            {
                Target = target,
                OtpCode = otpCode,
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                IsVerified = false,
            };

            await _unitOfWork.Otps.AddAsync(entity);

            await _unitOfWork.SaveChangesAsync();

            if (IsEmail(target))
            {
                var template = purpose switch
                {
                    EmailPurpose.Register => "RegisterOtpTemplate",

                    EmailPurpose.ForgotPassword => "ForgotPasswordOtpTemplate",

                    _ => "RegisterOtpTemplate",
                };

                var html = await _templateEngine.RenderEmailTemplateAsync(
                    template,
                    new OtpEmailModel { Otp = otpCode }
                );

                await _emailQueue.QueueEmailAsync(
                    new EmailMessage
                    {
                        To = target,
                        Subject = "OTP Code",
                        Body = html,
                    }
                );
            }

            // SMS TODO
            if (IsPhone(target))
            {
                // send sms later
            }

            return OperationResult.Success("OTP sent successfully");
        }

        public async Task<OperationResult> VerifyOtpAsync(string target, string otpCode)
        {
            var otp = await _unitOfWork.Otps.GetLatestOtpAsync(target, otpCode);

            if (otp == null)
                return OperationResult.Failure("Invalid OTP");

            if (otp.ExpiryDate < DateTime.UtcNow)
                return OperationResult.Failure("OTP expired");

            otp.IsUsed = true;
            otp.IsVerified = true;

            await _unitOfWork.SaveChangesAsync();

            return OperationResult.Success("OTP verified successfully");
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
