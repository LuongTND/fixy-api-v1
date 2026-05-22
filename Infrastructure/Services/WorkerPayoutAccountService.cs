using Application.Common;
using Application.DTOs.Payout;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services
{
    public class WorkerPayoutAccountService : IWorkerPayoutAccountService
    {
        private readonly IWorkerPayoutAccountRepository _repository;

        private readonly IUnitOfWork _unitOfWork;

        public WorkerPayoutAccountService(
            IWorkerPayoutAccountRepository repository,
            IUnitOfWork unitOfWork
        )
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<WorkerPayoutAccount>> CreateAsync(
            Guid workerId,
            CreateWorkerPayoutAccountDto dto,
            CancellationToken cancellationToken
        )
        {
            var hasDefault = await _repository.GetDefaultAsync(workerId, cancellationToken);

            var entity = new WorkerPayoutAccount
            {
                WorkerId = workerId,

                Method = WorkerPayoutMethod.Bank,

                AccountNumber = dto.AccountNumber,

                AccountName = dto.AccountName,

                BankName = dto.BankName,

                BankCode = dto.BankCode,

                IsDefault = hasDefault == null,
            };

            await _repository.AddAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<WorkerPayoutAccount>.Success(entity);
        }

        public async Task<OperationResult<List<WorkerPayoutAccountDto>>> GetMyAccountsAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var accounts = await _repository.GetByWorkerIdAsync(workerId, cancellationToken);

            return OperationResult<List<WorkerPayoutAccountDto>>.Success(
                accounts
                    .Select(x => new WorkerPayoutAccountDto
                    {
                        Id = x.Id,

                        AccountNumber = x.AccountNumber,

                        AccountName = x.AccountName,

                        BankName = x.BankName,

                        BankCode = x.BankCode,

                        IsDefault = x.IsDefault,

                        IsVerified = x.IsVerified,
                    })
                    .ToList()
            );
        }

        public async Task<OperationResult> SetDefaultAsync(
            Guid workerId,
            Guid payoutAccountId,
            CancellationToken cancellationToken
        )
        {
            var accounts = await _repository.GetByWorkerIdAsync(workerId, cancellationToken);

            var target = accounts.FirstOrDefault(x => x.Id == payoutAccountId);

            if (target == null)
            {
                return OperationResult.Failure("Payout account not found");
            }

            foreach (var item in accounts)
            {
                item.IsDefault = false;
            }

            target.IsDefault = true;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Default payout account updated");
        }

        public async Task<OperationResult> DeleteAsync(
            Guid workerId,
            Guid payoutAccountId,
            CancellationToken cancellationToken
        )
        {
            var entity = await _repository.GetByIdAsync(payoutAccountId, cancellationToken);

            if (entity == null || entity.WorkerId != workerId)
            {
                return OperationResult.Failure("Payout account not found");
            }

            _repository.Remove(entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Delete payout account successfully");
        }
    }
}
