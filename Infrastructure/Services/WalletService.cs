using Application.Common;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
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

    public async Task<OperationResult<Wallet>> GetWalletAsync(
        Guid userId,
        WalletOwnerType type,
        CancellationToken cancellationToken
    )
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, type, cancellationToken);

        if (wallet == null)
            return OperationResult<Wallet>.Failure("Wallet not found");

        return OperationResult<Wallet>.Success(wallet, "Get wallet successfully");
    }

    public async Task<OperationResult<List<WalletTransaction>>> GetWalletTransactionAsync(
        Guid userId,
        WalletOwnerType type,
        CancellationToken cancellationToken
    )
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId, type, cancellationToken);

        if (wallet == null)
            return OperationResult<List<WalletTransaction>>.Failure("Wallet not found");

        return OperationResult<List<WalletTransaction>>.Success(
            wallet.Transactions.ToList(),
            "Get wallet transcation successfully"
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
