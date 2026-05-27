using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.VoucherCampaign;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Voucher;
using AutoMapper;
using Domain.Entity;
using Domain.Enum;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Vouchers
{
    public class VoucherCampaignService : IVoucherCampaignService
    {
        private readonly IVoucherCampaignRepository _campaignRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VoucherCampaignService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public VoucherCampaignService(
            IVoucherCampaignRepository campaignRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<VoucherCampaignService> logger,
            ICurrentUserService currentUserService)
        {
            _campaignRepository = campaignRepository ?? throw new ArgumentNullException(nameof(campaignRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        public async Task<OperationResult<CampaignDto>> CreateAsync(CreateCampaignDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new Voucher Campaign: {Name}", dto.Name);

            var campaign = _mapper.Map<VoucherCampaign>(dto);
            campaign.Id = Guid.NewGuid();
            campaign.BudgetUsed = 0;

            await _campaignRepository.AddAsync(campaign, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = _mapper.Map<CampaignDto>(campaign);
            return OperationResult<CampaignDto>.Success(result, "Voucher campaign created successfully");
        }

        public async Task<OperationResult<CampaignDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null)
            {
                return OperationResult<CampaignDto>.Failure("Voucher campaign not found");
            }

            var result = _mapper.Map<CampaignDto>(campaign);
            return OperationResult<CampaignDto>.Success(result, "Voucher campaign retrieved successfully");
        }

        public async Task<OperationResult<PagedResponse<CampaignDto>>> GetPagedAsync(CampaignQuery query, CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _campaignRepository.GetPagedAsync(
                query.SearchTerm,
                query.PageNumber,
                query.PageSize,
                cancellationToken);

            var dtos = _mapper.Map<List<CampaignDto>>(items);
            var pagedResponse = new PagedResponse<CampaignDto>
            {
                Items = dtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };

            return OperationResult<PagedResponse<CampaignDto>>.Success(pagedResponse, "Voucher campaigns retrieved successfully");
        }

        public async Task<OperationResult<CampaignDto>> UpdateAsync(Guid id, UpdateCampaignDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating Voucher Campaign: {Id}", id);

            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null)
            {
                return OperationResult<CampaignDto>.Failure("Voucher campaign not found");
            }

            _mapper.Map(dto, campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = _mapper.Map<CampaignDto>(campaign);
            return OperationResult<CampaignDto>.Success(result, "Voucher campaign updated successfully");
        }

        public async Task<OperationResult<CampaignDto>> UpdateStatusAsync(Guid id, UpdateCampaignStatusDto dto, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating status of Campaign {Id} to {Status}", id, dto.Status);

            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null)
            {
                return OperationResult<CampaignDto>.Failure("Voucher campaign not found");
            }

            campaign.Status = dto.Status;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = _mapper.Map<CampaignDto>(campaign);
            return OperationResult<CampaignDto>.Success(result, $"Voucher campaign status updated to {dto.Status} successfully");
        }

        public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Soft deleting Voucher Campaign: {Id}", id);

            var campaign = await _campaignRepository.GetByIdAsync(id, cancellationToken);
            if (campaign == null)
            {
                return OperationResult.Failure("Voucher campaign not found");
            }

            campaign.DeletedBy = _currentUserService.UserName ?? _currentUserService.Email ?? "System";

            _campaignRepository.Remove(campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Voucher campaign deleted successfully");
        }
    }
}
