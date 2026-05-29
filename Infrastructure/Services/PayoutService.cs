using Application.Common;
using Application.DTOs.Payout;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

        public PayoutService(
            IWorkerProfileRepository workerProfileRepository,
            IWalletRepository walletRepository,
            IPayoutRequestRepository payoutRequestRepository,
            IWorkerPayoutAccountRepository workerPayoutAccountRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork
        )
        {
            _workerProfileRepository = workerProfileRepository;
            _walletRepository = walletRepository;
            _payoutRequestRepository = payoutRequestRepository;
            _workerPayoutAccountRepository = workerPayoutAccountRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
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

                wallet.LifetimeSpent += amount;

                var request = new PayoutRequest
                {
                    WorkerProfileId = workerProfile.Id,
                    PayoutAccountId = payoutAccountId,
                    Amount = amount,
                    Status = PayoutRequestStatus.Pending,
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
                        AccountName = payoutAccount.AccountName,
                        AccountNumber = payoutAccount.AccountNumber,
                        BankName = payoutAccount.BankName,
                        Amount = request.Amount,
                        CreatedDate = request.CreatedDate,
                        RejectReason = request.RejectReason,
                        Status = request.Status.ToString(),
                        TransferredAt = request.TransferredAt,
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
                            Amount = x.Amount,
                            Status = x.Status.ToString(),
                            RejectReason = x.RejectReason,
                            CreatedDate = x.CreatedDate,
                            TransferredAt = x.TransferredAt,

                            AccountNumber = x.PayoutAccount!.AccountNumber,

                            AccountName = x.PayoutAccount.AccountName,

                            BankName = x.PayoutAccount.BankName,
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
                            Amount = x.Amount,
                            Status = x.Status.ToString(),
                            RejectReason = x.RejectReason,
                            CreatedDate = x.CreatedDate,
                            TransferredAt = x.TransferredAt,

                            AccountNumber = x.PayoutAccount!.AccountNumber,

                            AccountName = x.PayoutAccount.AccountName,

                            BankName = x.PayoutAccount.BankName,
                        })
                        .ToList(),

                    TotalCount = result.Item2,

                    PageNumber = query.PageNumber,

                    PageSize = query.PageSize,
                }
            );
        }
    }
}
