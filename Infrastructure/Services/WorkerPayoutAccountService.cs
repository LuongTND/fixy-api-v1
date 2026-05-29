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
        private readonly IWorkerPayoutAccountRepository _worrkerPayoutAccountRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;

        private readonly IUnitOfWork _unitOfWork;

        public WorkerPayoutAccountService(
            IWorkerPayoutAccountRepository workerPayoutAccountRepository,
            IWorkerProfileRepository workerProfileRepository,
            IUnitOfWork unitOfWork
        )
        {
            _worrkerPayoutAccountRepository = workerPayoutAccountRepository;
            _workerProfileRepository = workerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<WorkerPayoutAccountDto>> CreateAsync(
            Guid workerId,
            CreateWorkerPayoutAccountDto dto,
            CancellationToken cancellationToken
        )
        {
            var workerProfile = await _workerProfileRepository.GetWorkerProfileByUserIdAsync(
                workerId,
                cancellationToken
            );
            if (workerProfile == null)
            {
                return OperationResult<WorkerPayoutAccountDto>.Failure("Worker profile not found");
            }
            var hasDefault = await _worrkerPayoutAccountRepository.GetDefaultAsync(
                workerId,
                cancellationToken
            );

            var entity = new WorkerPayoutAccount
            {
                WorkerProfileId = workerProfile.Id,

                Method = WorkerPayoutMethod.Bank,

                AccountNumber = dto.AccountNumber,

                AccountName = dto.AccountName,

                BankName = dto.BankName,

                BankCode = dto.BankCode,

                IsDefault = hasDefault == null,
            };

            await _worrkerPayoutAccountRepository.AddAsync(entity, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<WorkerPayoutAccountDto>.Success(
                new WorkerPayoutAccountDto
                {
                    Id = entity.Id,

                    AccountNumber = entity.AccountNumber,

                    AccountName = entity.AccountName,

                    BankName = entity.BankName,

                    BankCode = entity.BankCode,

                    IsDefault = entity.IsDefault,

                    IsVerified = entity.IsVerified,
                },
                "Create payout account successfully"
            );
        }

        public async Task<OperationResult<List<WorkerPayoutAccountDto>>> GetMyAccountsAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var accounts = await _worrkerPayoutAccountRepository.GetByWorkerIdAsync(
                workerId,
                cancellationToken
            );

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
                    .ToList(),
                "Get payout account list successfully"
            );
        }

        public async Task<OperationResult> SetDefaultAsync(
            Guid workerId,
            Guid payoutAccountId,
            CancellationToken cancellationToken
        )
        {
            var accounts = await _worrkerPayoutAccountRepository.GetByWorkerIdAsync(
                workerId,
                cancellationToken
            );

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
            var entity = await _worrkerPayoutAccountRepository.GetByIdAsync(
                payoutAccountId,
                cancellationToken
            );
            var workerProfile = await _workerProfileRepository.GetWorkerProfileByUserIdAsync(
                workerId,
                cancellationToken
            );
            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            if (entity == null || entity.WorkerProfileId != workerProfile.Id)
            {
                return OperationResult.Failure("Payout account not found");
            }

            _worrkerPayoutAccountRepository.Remove(entity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Delete payout account successfully");
        }
    }
}
