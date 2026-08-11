using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.Payout;
using Application.DTOs.Payment;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class PayoutService : IPayoutService
    {
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IPayoutRequestRepository _payoutRequestRepository;
        private readonly IWorkerPayoutAccountRepository _workerPayoutAccountRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayoutService> _logger;

        private static readonly Regex PayoutCodeRegex = new(@"WD[A-Z0-9]{4,8}", RegexOptions.Compiled);

        public PayoutService(
            IWorkerProfileRepository workerProfileRepository,
            IWalletRepository walletRepository,
            IPayoutRequestRepository payoutRequestRepository,
            IWorkerPayoutAccountRepository workerPayoutAccountRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IHubContext<NotificationHub> hubContext,
            IConfiguration configuration,
            ILogger<PayoutService> logger
        )
        {
            _workerProfileRepository = workerProfileRepository;
            _walletRepository = walletRepository;
            _payoutRequestRepository = payoutRequestRepository;
            _workerPayoutAccountRepository = workerPayoutAccountRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OperationResult<PayoutRequestDto>> CreateRequestAsync(
            Guid workerId,
            Guid payoutAccountId,
            long amount,
            CancellationToken cancellationToken
        )
        {
            if (amount <= 0)
            {
                return OperationResult<PayoutRequestDto>.Failure("Invalid amount");
            }

            var existsPending = await _payoutRequestRepository.ExistsPendingRequestAsync(
                workerId,
                cancellationToken
            );

            if (existsPending)
            {
                return OperationResult<PayoutRequestDto>.Failure(
                    "You already have a pending payout request"
                );
            }

            var payoutAccount = await _workerPayoutAccountRepository.GetByIdAsync(
                payoutAccountId,
                cancellationToken
            );
            var workerProfile = await _workerProfileRepository.GetWorkerProfileByUserIdAsync(
                workerId,
                cancellationToken
            );
            if (workerProfile == null)
            {
                return OperationResult<PayoutRequestDto>.Failure("Worker profile not found");
            }
            if (payoutAccount == null || workerProfile.UserId != workerId)
            {
                return OperationResult<PayoutRequestDto>.Failure("Payout account not found");
            }

            var wallet = await _walletRepository.GetByUserIdAsync(
                workerId,
                WalletOwnerType.Worker,
                cancellationToken
            );

            if (wallet == null)
            {
                return OperationResult<PayoutRequestDto>.Failure("Wallet not found");
            }

            if (wallet.Balance < amount)
            {
                return OperationResult<PayoutRequestDto>.Failure("Insufficient balance");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var before = wallet.Balance;

                // HOLD MONEY
                wallet.Balance -= amount;

                // Generate unique PayoutCode (WD + 6 random alphanumeric chars)
                var payoutCode = await GenerateUniquePayoutCodeAsync(cancellationToken);

                // Generate VietQR Dynamic URL
                var vietQrUrl = GenerateVietQrUrl(
                    payoutAccount.BankCode ?? "",
                    payoutAccount.AccountNumber,
                    amount,
                    payoutCode,
                    payoutAccount.AccountName
                );

                var request = new PayoutRequest
                {
                    WorkerProfileId = workerProfile.Id,
                    PayoutAccountId = payoutAccountId,
                    Amount = amount,
                    Status = PayoutRequestStatus.Pending,
                    PayoutCode = payoutCode,
                    VietQrUrl = vietQrUrl,
                };

                await _payoutRequestRepository.AddAsync(request, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var tx = new WalletTransaction
                {
                    WalletId = wallet.Id,

                    PayoutRequestId = request.Id,

                    Type = WalletTransactionType.Withdrawal,
                    Direction = WalletDirection.Debit,

                    Amount = amount,

                    BalanceBefore = before,
                    BalanceAfter = wallet.Balance,

                    Status = TransactionStatus.Pending,
                };

                await _walletTransactionRepository.AddAsync(tx, cancellationToken);

                _walletRepository.Update(wallet);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync();

                return OperationResult<PayoutRequestDto>.Success(
                    new PayoutRequestDto
                    {
                        Id = request.Id,
                        PayoutCode = request.PayoutCode,
                        AccountName = payoutAccount.AccountName,
                        AccountNumber = payoutAccount.AccountNumber,
                        BankName = payoutAccount.BankName,
                        BankCode = payoutAccount.BankCode,
                        Amount = request.Amount,
                        CreatedDate = request.CreatedDate,
                        RejectReason = request.RejectReason,
                        Status = request.Status.ToString(),
                        TransferredAt = request.TransferredAt,
                        VietQrUrl = request.VietQrUrl,
                    },
                    "Payout request created successfully"
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                await _unitOfWork.RollbackTransactionAsync();

                return OperationResult<PayoutRequestDto>.Failure("Wallet conflict, retry again");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OperationResult> ApproveAsync(
            Guid payoutRequestId,
            Guid reviewerId,
            CancellationToken cancellationToken
        )
        {
            var request = await _payoutRequestRepository.GetByIdWithDetailsAsync(
                payoutRequestId,
                cancellationToken
            );

            if (request == null)
            {
                return OperationResult.Failure("Payout request not found");
            }

            if (request.Status != PayoutRequestStatus.Pending)
            {
                return OperationResult.Failure("Payout request already processed");
            }
            var wallet = await _walletRepository.GetByUserIdAsync(
                request.WorkerProfile!.UserId,
                WalletOwnerType.Worker,
                cancellationToken
            );

            if (wallet == null)
            {
                return OperationResult.Failure("Wallet not found");
            }
            wallet.LifetimeSpent += request.Amount;
            _walletRepository.Update(wallet);

            var tx = request.WalletTransactions.FirstOrDefault(x =>
                x.Type == WalletTransactionType.Withdrawal
            );

            if (tx == null)
            {
                return OperationResult.Failure("Withdrawal transaction not found");
            }

            request.Status = PayoutRequestStatus.Approved;

            request.ReviewedById = reviewerId;

            request.TransferredAt = DateTime.UtcNow;

            tx.Status = TransactionStatus.Success;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Payout approved successfully");
        }

        public async Task<OperationResult> RejectAsync(
            Guid payoutRequestId,
            Guid reviewerId,
            string reason,
            CancellationToken cancellationToken
        )
        {
            var request = await _payoutRequestRepository.GetByIdWithDetailsAsync(
                payoutRequestId,
                cancellationToken
            );

            if (request == null)
            {
                return OperationResult.Failure("Payout request not found");
            }

            if (request.Status != PayoutRequestStatus.Pending)
            {
                return OperationResult.Failure("Payout request already processed");
            }

            var tx = request.WalletTransactions.FirstOrDefault(x =>
                x.Type == WalletTransactionType.Withdrawal
            );

            if (tx == null)
            {
                return OperationResult.Failure("Withdrawal transaction not found");
            }

            var wallet = await _walletRepository.GetByIdAsync(tx.WalletId, cancellationToken);

            if (wallet == null)
            {
                return OperationResult.Failure("Wallet not found");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var before = wallet.Balance;

                // RETURN MONEY
                wallet.Balance += request.Amount;

                request.Status = PayoutRequestStatus.Rejected;

                request.ReviewedById = reviewerId;

                request.RejectReason = reason;

                tx.Status = TransactionStatus.Failed;

                var refundTx = new WalletTransaction
                {
                    WalletId = wallet.Id,

                    PayoutRequestId = request.Id,

                    Type = WalletTransactionType.Refund,
                    Direction = WalletDirection.Credit,

                    Amount = request.Amount,

                    BalanceBefore = before,
                    BalanceAfter = wallet.Balance,

                    Status = TransactionStatus.Success,
                };

                await _walletTransactionRepository.AddAsync(refundTx, cancellationToken);

                _walletRepository.Update(wallet);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync();

                return OperationResult.Success("Payout rejected successfully");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OperationResult<PagedResponse<PayoutRequestDto>>> GetAllAsync(
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _payoutRequestRepository.GetPagedAsync(query, cancellationToken);

            return OperationResult<PagedResponse<PayoutRequestDto>>.Success(
                new PagedResponse<PayoutRequestDto>
                {
                    Items = result
                        .Item1.Select(x => new PayoutRequestDto
                        {
                            Id = x.Id,
                            PayoutCode = x.PayoutCode,
                            Amount = x.Amount,
                            Status = x.Status.ToString(),
                            RejectReason = x.RejectReason,
                            GatewayTransactionRef = x.GatewayTransactionRef,
                            VietQrUrl = x.VietQrUrl,
                            CreatedDate = x.CreatedDate,
                            TransferredAt = x.TransferredAt,

                            AccountNumber = x.PayoutAccount!.AccountNumber,

                            AccountName = x.PayoutAccount.AccountName,

                            BankName = x.PayoutAccount.BankName,

                            BankCode = x.PayoutAccount.BankCode,
                        })
                        .ToList(),

                    TotalCount = result.Item2,

                    PageNumber = query.PageNumber,

                    PageSize = query.PageSize,
                }
            );
        }

        public async Task<OperationResult<PagedResponse<PayoutRequestDto>>> GetMyRequestsAsync(
            Guid workerId,
            PagedQuery query,
            CancellationToken cancellationToken
        )
        {
            var result = await _payoutRequestRepository.GetWorkerPagedAsync(
                workerId,
                query,
                cancellationToken
            );

            return OperationResult<PagedResponse<PayoutRequestDto>>.Success(
                new PagedResponse<PayoutRequestDto>
                {
                    Items = result
                        .Item1.Select(x => new PayoutRequestDto
                        {
                            Id = x.Id,
                            PayoutCode = x.PayoutCode,
                            Amount = x.Amount,
                            Status = x.Status.ToString(),
                            RejectReason = x.RejectReason,
                            GatewayTransactionRef = x.GatewayTransactionRef,
                            VietQrUrl = x.VietQrUrl,
                            CreatedDate = x.CreatedDate,
                            TransferredAt = x.TransferredAt,

                            AccountNumber = x.PayoutAccount!.AccountNumber,

                            AccountName = x.PayoutAccount.AccountName,

                            BankName = x.PayoutAccount.BankName,

                            BankCode = x.PayoutAccount.BankCode,
                        })
                        .ToList(),

                    TotalCount = result.Item2,

                    PageNumber = query.PageNumber,

                    PageSize = query.PageSize,
                }
            );
        }

        public async Task<OperationResult> ProcessSePayWebhookAsync(
            SePayWebhookDto webhook,
            string? authorizationHeader,
            CancellationToken cancellationToken
        )
        {
            // 1. Validate secret token
            var expectedToken = _configuration["SePay:WebhookToken"];
            if (!string.IsNullOrEmpty(expectedToken))
            {
                var receivedToken = authorizationHeader?.Replace("Apikey ", "", StringComparison.OrdinalIgnoreCase).Trim();
                if (!string.Equals(receivedToken, expectedToken, StringComparison.Ordinal))
                {
                    _logger.LogWarning("SePay webhook received with invalid secret token.");
                    return OperationResult.Failure("Unauthorized");
                }
            }

            // 2. Only process outbound transfers (money out)
            if (!string.Equals(webhook.TransferType, "out", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "SePay webhook ignored: TransferType={TransferType} (not 'out')",
                    webhook.TransferType
                );
                return OperationResult.Success("Ignored: not an outbound transfer");
            }

            // 3. Extract PayoutCode from transfer content using Regex
            var match = PayoutCodeRegex.Match(webhook.Content?.ToUpperInvariant() ?? "");
            if (!match.Success)
            {
                _logger.LogInformation(
                    "SePay webhook ignored: No PayoutCode found in content '{Content}'",
                    webhook.Content
                );
                return OperationResult.Success("Ignored: no matching payout code found");
            }

            var payoutCode = match.Value;

            // 4. Find the PayoutRequest by PayoutCode
            var request = await _payoutRequestRepository.GetByPayoutCodeWithDetailsAsync(
                payoutCode,
                cancellationToken
            );

            if (request == null)
            {
                _logger.LogWarning(
                    "SePay webhook: PayoutRequest not found for code '{PayoutCode}'",
                    payoutCode
                );
                return OperationResult.Failure($"PayoutRequest not found for code {payoutCode}");
            }

            // 5. Idempotency check: already approved -> return success
            if (request.Status == PayoutRequestStatus.Approved)
            {
                _logger.LogInformation(
                    "SePay webhook: PayoutRequest '{PayoutCode}' already approved (idempotency)",
                    payoutCode
                );
                return OperationResult.Success("Already processed");
            }

            if (request.Status != PayoutRequestStatus.Pending)
            {
                _logger.LogWarning(
                    "SePay webhook: PayoutRequest '{PayoutCode}' status is {Status}, cannot process",
                    payoutCode,
                    request.Status
                );
                return OperationResult.Failure("Payout request is not in Pending status");
            }

            // 6. Update PayoutRequest status
            var wallet = await _walletRepository.GetByUserIdAsync(
                request.WorkerProfile!.UserId,
                WalletOwnerType.Worker,
                cancellationToken
            );

            if (wallet == null)
            {
                return OperationResult.Failure("Worker wallet not found");
            }

            request.Status = PayoutRequestStatus.Approved;
            request.TransferredAt = DateTime.UtcNow;
            request.GatewayTransactionRef = webhook.ReferenceCode ?? webhook.Id.ToString();

            wallet.LifetimeSpent += request.Amount;
            _walletRepository.Update(wallet);

            var tx = request.WalletTransactions.FirstOrDefault(x =>
                x.Type == WalletTransactionType.Withdrawal
            );
            if (tx != null)
            {
                tx.Status = TransactionStatus.Success;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SePay webhook: PayoutRequest '{PayoutCode}' approved. GatewayRef={GatewayRef}, Amount={Amount}",
                payoutCode,
                request.GatewayTransactionRef,
                request.Amount
            );

            // 7. Send real-time notifications
            try
            {
                // Notify KTV via FCM + SignalR
                await _notificationService.SendNotificationAsync(
                    request.WorkerProfile.UserId,
                    NotificationType.Payment,
                    "Rút tiền thành công! 💸",
                    $"{request.Amount:N0}đ đã về tài khoản. Mã ngân hàng: {request.GatewayTransactionRef}",
                    deepLink: null,
                    meta: new { payoutRequestId = request.Id, payoutCode = request.PayoutCode },
                    code: null,
                    cancellationToken
                );

                // Broadcast to Web Admin dashboard via SignalR
                await _hubContext.Clients.All.SendAsync(
                    "PayoutApproved",
                    new
                    {
                        payoutRequestId = request.Id,
                        payoutCode = request.PayoutCode,
                        gatewayTransactionRef = request.GatewayTransactionRef,
                        amount = request.Amount,
                        status = request.Status.ToString(),
                    },
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "SePay webhook: Failed to send notifications for PayoutCode '{PayoutCode}'",
                    payoutCode
                );
                // Don't fail the webhook response due to notification errors
            }

            return OperationResult.Success("Payout approved via SePay webhook");
        }

        // --- Helper Methods ---

        private async Task<string> GenerateUniquePayoutCodeAsync(CancellationToken cancellationToken)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = "WD" + new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());

                var existing = await _payoutRequestRepository.FirstOrDefaultAsync(
                    x => x.PayoutCode == code,
                    cancellationToken
                );

                if (existing == null)
                {
                    return code;
                }
            }

            // Fallback: use timestamp-based code
            return "WD" + DateTime.UtcNow.ToString("HHmmss");
        }

        private static string GenerateVietQrUrl(
            string bankCode,
            string accountNumber,
            long amount,
            string payoutCode,
            string accountName
        )
        {
            var addInfo = Uri.EscapeDataString($"FIXY RUT {payoutCode}");
            var encodedAccountName = Uri.EscapeDataString(accountName);
            return $"https://img.vietqr.io/image/{bankCode}-{accountNumber}-compact2.png?amount={amount}&addInfo={addInfo}&accountName={encodedAccountName}";
        }
    }
}
