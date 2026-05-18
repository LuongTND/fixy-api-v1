using Application.Common;
using Application.DTOs.Wallet;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<WalletOverviewDto>> GetWalletOverviewAsync(
        Guid userId,
        WalletOwnerType type,
        CancellationToken cancellationToken
    )
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, type, cancellationToken);

        if (wallet == null)
            return OperationResult<WalletOverviewDto>.Failure("Wallet not found");

        var recentTransactions = await _walletTransactionRepository.GetRecentByWalletIdAsync(
            wallet.Id,
            10,
            cancellationToken
        );

        return OperationResult<WalletOverviewDto>.Success(
            new WalletOverviewDto
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                LifetimeEarned = wallet.LifetimeEarned,
                LifetimeSpent = wallet.LifetimeSpent,
                RecentTransactions = recentTransactions
                    .Select(t => new WalletTransactionDto
                    {
                        Id = t.Id,
                        Type = t.Type.ToString(),
                        Direction = t.Direction.ToString(),
                        Amount = t.Amount,
                        BalanceAfter = t.BalanceAfter,
                        BalanceBefore = t.BalanceBefore,
                        CreatedDate = t.CreatedDate,
                        Status = t.Status.ToString(),
                    })
                    .ToList(),
            },
            "Get wallet overview successfully"
        );
    }

    public async Task<
        OperationResult<PagedResponse<WalletTransactionDto>>
    > GetWalletTransactionsAsync(
        Guid userId,
        WalletOwnerType type,
        PagedQuery query,
        CancellationToken cancellationToken
    )
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, type, cancellationToken);

        if (wallet == null)
        {
            return OperationResult<PagedResponse<WalletTransactionDto>>.Failure("Wallet not found");
        }

        var result = await _walletTransactionRepository.GetPagedByWalletIdAsync(
            wallet.Id,
            query,
            cancellationToken
        );

        return OperationResult<PagedResponse<WalletTransactionDto>>.Success(
            new PagedResponse<WalletTransactionDto>
            {
                Items = result
                    .Item1.Select(t => new WalletTransactionDto
                    {
                        Id = t.Id,
                        Type = t.Type.ToString(),
                        Direction = t.Direction.ToString(),
                        Amount = t.Amount,
                        BalanceAfter = t.BalanceAfter,
                        BalanceBefore = t.BalanceBefore,
                        CreatedDate = t.CreatedDate,
                        Status = t.Status.ToString(),
                    })
                    .ToList(),
                TotalCount = result.Item2,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
            },
            "Get transactions successfully"
        );
    }

    public async Task<OperationResult<WalletTransaction>> TopUpAsync(
        Guid userId,
        long amount,
        string externalId,
        CancellationToken cancellationToken
    )
    {
        if (await _walletTransactionRepository.ExistsExternalTransactionAsync(externalId))
            return OperationResult<WalletTransaction>.Failure("Duplicate transaction");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var wallet = await _walletRepository.GetByUserIdAsync(
                userId,
                WalletOwnerType.Customer,
                cancellationToken
            );

            if (wallet == null)
                return OperationResult<WalletTransaction>.Failure("Wallet not found");

            var before = wallet.Balance;

            wallet.Balance += amount;
            wallet.LifetimeEarned += amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.TopUp,
                Direction = WalletDirection.Credit,
                Amount = amount,
                BalanceBefore = before,
                BalanceAfter = wallet.Balance,
                Status = TransactionStatus.Success,
                ExternalTransactionId = externalId,
            };

            await _walletTransactionRepository.AddAsync(tx, cancellationToken);
            _walletRepository.Update(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<WalletTransaction>.Success(tx, "Topup success");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<WalletTransaction>.Failure("Wallet conflict, retry again");
        }
    }

    public async Task<OperationResult<WalletTransaction>> PayAsync(
        Guid userId,
        long amount,
        string referenceId,
        CancellationToken cancellationToken
    )
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var wallet = await _walletRepository.GetByUserIdAsync(
                userId,
                WalletOwnerType.Customer,
                cancellationToken
            );

            if (wallet == null)
                return OperationResult<WalletTransaction>.Failure("Wallet not found");

            if (wallet.Balance < amount)
                return OperationResult<WalletTransaction>.Failure("Insufficient balance");

            var before = wallet.Balance;

            wallet.Balance -= amount;
            wallet.LifetimeSpent += amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Payment,
                Direction = WalletDirection.Debit,
                Amount = amount,
                BalanceBefore = before,
                BalanceAfter = wallet.Balance,
                ReferenceId = referenceId,
                Status = TransactionStatus.Success,
            };

            await _walletTransactionRepository.AddAsync(tx, cancellationToken);
            _walletRepository.Update(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<WalletTransaction>.Success(tx, "Pay success");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<WalletTransaction>.Failure("Wallet conflict, retry again");
        }
    }

    public async Task<OperationResult<WalletTransaction>> RefundAsync(
        Guid userId,
        long amount,
        string referenceId,
        CancellationToken cancellationToken
    )
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var wallet = await _walletRepository.GetByUserIdAsync(
                userId,
                WalletOwnerType.Customer,
                cancellationToken
            );

            if (wallet == null)
                return OperationResult<WalletTransaction>.Failure("Wallet not found");

            var before = wallet.Balance;

            wallet.Balance += amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Refund,
                Direction = WalletDirection.Credit,
                Amount = amount,
                BalanceBefore = before,
                BalanceAfter = wallet.Balance,
                ReferenceId = referenceId,
                Status = TransactionStatus.Success,
            };

            await _walletTransactionRepository.AddAsync(tx, cancellationToken);
            _walletRepository.Update(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<WalletTransaction>.Success(tx, "Refund success");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<WalletTransaction>.Failure("Wallet conflict, retry again");
        }
    }

    public async Task<OperationResult<WalletTransaction>> WithdrawAsync(
        Guid userId,
        long amount,
        string referenceId,
        CancellationToken cancellationToken
    )
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var wallet = await _walletRepository.GetByUserIdAsync(
                userId,
                WalletOwnerType.Worker,
                cancellationToken
            );

            if (wallet == null)
                return OperationResult<WalletTransaction>.Failure("Wallet not found");

            if (wallet.Balance < amount)
                return OperationResult<WalletTransaction>.Failure("Insufficient balance");

            var before = wallet.Balance;

            // 🔥 HOLD MONEY (giữ logic đơn giản MVP)
            wallet.Balance -= amount;

            wallet.LifetimeSpent += amount;

            var tx = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.Withdrawal,
                Direction = WalletDirection.Debit,
                Amount = amount,
                BalanceBefore = before,
                BalanceAfter = wallet.Balance,
                ReferenceId = referenceId,
                Status = TransactionStatus.Pending,
            };

            await _walletTransactionRepository.AddAsync(tx, cancellationToken);
            _walletRepository.Update(wallet);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync();

            return OperationResult<WalletTransaction>.Success(tx, "Withdraw requested");
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return OperationResult<WalletTransaction>.Failure("Wallet conflict, retry again");
        }
    }
}
