using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Application.Common;
using Application.Common.Models.Email;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Email;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services.Email
{
    public class OtpService : IOtpService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailQueue _emailQueue;
        private readonly ITemplateEngine _templateEngine;

        public OtpService(
            IUserRepository userRepository,
            IUserOtpRepository userOtpRepository,
            IUnitOfWork unitOfWork,
            IEmailQueue emailQueue,
            ITemplateEngine templateEngine
        )
        {
            _userRepository = userRepository;
            _userOtpRepository = userOtpRepository;
            _unitOfWork = unitOfWork;
            _emailQueue = emailQueue;
            _templateEngine = templateEngine;
        }

        public async Task<OperationResult> SendOtpAsync(
            string target,
            EmailPurpose purpose,
            CancellationToken cancellationToken
        )
        {
            target = NormalizeTarget(target);

            return await SendOtpInternalAsync(target, purpose, cancellationToken);
        }

        public async Task<OperationResult> VerifyOtpAsync(
            string target,
            string otpCode,
            CancellationToken cancellationToken
        )
        {
            target = NormalizeTarget(target);

            var otp = await _userOtpRepository.GetLatestOtpAsync(
                target,
                otpCode,
                cancellationToken
            );

            if (otp == null)
            {
                return OperationResult.Failure("Invalid OTP");
            }

            if (otp.IsUsed)
            {
                return OperationResult.Failure("OTP already used");
            }
            if (otp.IsVerified)
            {
                return OperationResult.Failure("OTP already verified");
            }
            if (otp.ExpiryDate < DateTime.UtcNow)
            {
                return OperationResult.Failure("OTP expired");
            }

            otp.IsUsed = true;
            otp.IsVerified = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("OTP verified successfully");
        }

        private async Task<OperationResult> SendOtpInternalAsync(
            string target,
            EmailPurpose purpose,
            CancellationToken cancellationToken
        )
        {
            if (!IsEmail(target) && !IsPhone(target))
            {
                return OperationResult.Failure("Invalid email or phone number");
            }

            var latestOtp = await _userOtpRepository.GetLatestOtpByTargetAsync(
                target,
                cancellationToken
            );

            if (latestOtp != null)
            {
                var nextRequestTime = latestOtp.CreatedDate.AddSeconds(60);

                if (nextRequestTime > DateTime.UtcNow)
                {
                    var remain = (int)(nextRequestTime - DateTime.UtcNow).TotalSeconds;

                    return OperationResult.Failure(
                        $"Please wait {remain}s before requesting another OTP"
                    );
                }
            }

            var existedUser = await _userRepository.GetByTargetAsync(target, cancellationToken);

            switch (purpose)
            {
                case EmailPurpose.Register:

                    if (existedUser != null)
                    {
                        return OperationResult.Failure("Email or phone number already used");
                    }

                    break;

                case EmailPurpose.ForgotPassword:

                    if (existedUser == null)
                    {
                        return OperationResult.Failure("User not found");
                    }

                    break;
            }

            var oldOtps = await _userOtpRepository.GetUnusedOtpsAsync(target, cancellationToken);

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var entity = new UserOtp
            {
                Target = target,
                OtpCode = otpCode,
                CreatedDate = DateTime.UtcNow,
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

                await _emailQueue.QueueEmailAsync(
                    new EmailMessage
                    {
                        To = target,
                        Subject = "Mã OTP của bạn",
                        Body = html,
                    }
                );
            }

            if (IsPhone(target))
            {
                // TODO: Send SMS
            }

            return OperationResult.Success("OTP sent successfully");
        }

        private string NormalizeTarget(string target)
        {
            return target.Trim().ToLower();
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
