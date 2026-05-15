using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Application.Common;
using Application.Common.Models.Email;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Email;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Repositories;

namespace Infrastructure.Services.Email
{
    public class OtpService : IOtpService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ITemplateEngine _templateEngine;

        public OtpService(
            IUserRepository userRepository,
            IUserOtpRepository userOtpRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ITemplateEngine templateEngine
        )
        {
            _userRepository = userRepository;
            _userOtpRepository = userOtpRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _templateEngine = templateEngine;
        }

        public async Task<OperationResult> SendOtpAsync(
            string target,
            EmailPurpose purpose,
            CancellationToken cancellationToken
        )
        {
            var existedUser = await _userRepository.GetByTargetAsync(target, cancellationToken);

            if (existedUser != null)
                return OperationResult.Failure("Email or phone number already used");
            var oldOtps = await _userOtpRepository.GetUnusedOtpsAsync(target);

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

            await _userOtpRepository.AddAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

                await _emailService.SendEmailAsync(target, "Mã OTP của bạn", html);
            }

            // SMS TODO
            if (IsPhone(target))
            {
                // send sms later
            }

            return OperationResult.Success("OTP sent successfully");
        }

        public async Task<OperationResult> VerifyOtpAsync(
            string target,
            string otpCode,
            CancellationToken cancellationToken
        )
        {
            var otp = await _userOtpRepository.GetLatestOtpAsync(
                target,
                otpCode,
                cancellationToken
            );

            if (otp == null)
                return OperationResult.Failure("Invalid OTP");

            if (otp.ExpiryDate < DateTime.UtcNow)
                return OperationResult.Failure("OTP expired");

            otp.IsUsed = true;
            otp.IsVerified = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
