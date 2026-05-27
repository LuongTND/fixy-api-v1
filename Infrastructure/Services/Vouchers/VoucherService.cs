using Application.Common;
using Application.Common.Interfaces;
using Application.DTOs.Voucher;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Voucher;
using AutoMapper;
using Domain.Entity;
using Domain.Enum;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Vouchers
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IVoucherUsageHistoryRepository _usageHistoryRepository;
        private readonly IBookingVoucherRepository _bookingVoucherRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VoucherService> _logger;
        private readonly ICurrentUserService _currentUserService;

        public VoucherService(
            IVoucherRepository voucherRepository,
            IVoucherUsageHistoryRepository usageHistoryRepository,
            IBookingVoucherRepository bookingVoucherRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<VoucherService> logger,
            ICurrentUserService currentUserService)
        {
            _voucherRepository = voucherRepository ?? throw new ArgumentNullException(nameof(voucherRepository));
            _usageHistoryRepository = usageHistoryRepository ?? throw new ArgumentNullException(nameof(usageHistoryRepository));
            _bookingVoucherRepository = bookingVoucherRepository ?? throw new ArgumentNullException(nameof(bookingVoucherRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        // =============================================
        // Admin CRUD Voucher
        // =============================================

        public async Task<OperationResult<VoucherDto>> CreateAsync(CreateVoucherDto dto, CancellationToken cancellationToken = default)
        {
            // Validate code uniqueness
            var codeExists = await _voucherRepository.IsCodeExistsAsync(dto.Code.Trim().ToUpper(), cancellationToken);
            if (codeExists)
            {
                return OperationResult<VoucherDto>.Failure("Voucher code already exists");
            }

            var voucherId = Guid.NewGuid();
            var voucher = _mapper.Map<Domain.Entity.Voucher>(dto);
            voucher.Id = voucherId;
            voucher.Code = dto.Code.Trim().ToUpper();
            voucher.Status = VoucherStatus.Draft;
            voucher.StartsAt = dto.StartsAt;
            voucher.ExpiresAt = dto.ExpiresAt;

            // Set CreatedById using ICurrentUserService
            if (_currentUserService.UserId != null && Guid.TryParse(_currentUserService.UserId, out var parsedUserId))
            {
                voucher.CreatedById = parsedUserId;
            }

            // Initialize Quota
            voucher.Quota = new VoucherQuota
            {
                Id = Guid.NewGuid(),
                VoucherId = voucherId,
                MaxUsage = dto.MaxUsage,
                MaxUsagePerUser = dto.MaxUsagePerUser,
                UsedCount = 0
            };

            // Initialize Category Restriction if provided
            if (dto.CategoryId.HasValue)
            {
                voucher.Restrictions.Add(new VoucherRestriction
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    Type = RestrictionType.Category,
                    Value = dto.CategoryId.Value.ToString()
                });
            }

            // Initialize City Restriction if provided
            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                voucher.Restrictions.Add(new VoucherRestriction
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    Type = RestrictionType.City,
                    Value = dto.City.Trim()
                });
            }

            // Initialize First Order Restriction if provided
            if (dto.FirstOrderOnly == true)
            {
                voucher.Restrictions.Add(new VoucherRestriction
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    Type = RestrictionType.IsFirstOrder,
                    Value = "true"
                });
            }

            await _voucherRepository.AddAsync(voucher, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher created. Code: {VoucherCode}, Id: {VoucherId}", voucher.Code, voucher.Id);

            return OperationResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher), "Voucher created successfully");
        }

        public async Task<OperationResult<VoucherDto>> GetByIdAsync(Guid id,CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
            if (voucher == null)
            {
                return OperationResult<VoucherDto>.Failure("Voucher not found");
            }

            return OperationResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher),"Voucher retrieved successfully");
        }

        public async Task<OperationResult<PagedResponse<VoucherDto>>> GetPagedAsync(VoucherQuery query,CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _voucherRepository.GetPagedAsync(
                query.SearchTerm,
                query.PageNumber,
                query.PageSize,
                cancellationToken);

            var response = new PagedResponse<VoucherDto>
            {
                Items = _mapper.Map<List<VoucherDto>>(items),
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount,
            };

            return OperationResult<PagedResponse<VoucherDto>>.Success(response,"Vouchers retrieved successfully");
        }

        public async Task<OperationResult<VoucherDto>> UpdateAsync(Guid id,UpdateVoucherDto dto,CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
            if (voucher == null)
            {
                return OperationResult<VoucherDto>.Failure("Voucher not found");
            }

            // Apply partial update
            if (dto.Value.HasValue) voucher.Value = dto.Value.Value;
            if (dto.MinOrderValue.HasValue) voucher.MinOrderValue = dto.MinOrderValue.Value;
            if (dto.MaxDiscount.HasValue) voucher.MaxDiscount = dto.MaxDiscount.Value;
            if (dto.StartsAt.HasValue) voucher.StartsAt = dto.StartsAt.Value;
            if (dto.ExpiresAt.HasValue) voucher.ExpiresAt = dto.ExpiresAt.Value;
            if (dto.Description != null) voucher.Description = dto.Description;

            // Update Quota if provided
            if (voucher.Quota == null)
            {
                voucher.Quota = new VoucherQuota
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucher.Id,
                    UsedCount = 0
                };
            }
            if (dto.MaxUsage.HasValue) voucher.Quota.MaxUsage = dto.MaxUsage.Value;
            if (dto.MaxUsagePerUser.HasValue) voucher.Quota.MaxUsagePerUser = dto.MaxUsagePerUser.Value;

            // Update Category Restriction if provided
            if (dto.CategoryId.HasValue)
            {
                var catRestriction = voucher.Restrictions.FirstOrDefault(r => r.Type == RestrictionType.Category);
                if (catRestriction != null)
                {
                    catRestriction.Value = dto.CategoryId.Value.ToString();
                }
                else
                {
                    voucher.Restrictions.Add(new VoucherRestriction
                    {
                        Id = Guid.NewGuid(),
                        VoucherId = voucher.Id,
                        Type = RestrictionType.Category,
                        Value = dto.CategoryId.Value.ToString()
                    });
                }
            }

            // Update City Restriction if provided
            if (dto.City != null)
            {
                var cityRestriction = voucher.Restrictions.FirstOrDefault(r => r.Type == RestrictionType.City);
                if (!string.IsNullOrWhiteSpace(dto.City))
                {
                    if (cityRestriction != null)
                    {
                        cityRestriction.Value = dto.City.Trim();
                    }
                    else
                    {
                        voucher.Restrictions.Add(new VoucherRestriction
                        {
                            Id = Guid.NewGuid(),
                            VoucherId = voucher.Id,
                            Type = RestrictionType.City,
                            Value = dto.City.Trim()
                        });
                    }
                }
                else
                {
                    if (cityRestriction != null)
                    {
                        voucher.Restrictions.Remove(cityRestriction);
                    }
                }
            }

            // Update First Order Restriction if provided
            if (dto.FirstOrderOnly.HasValue)
            {
                var firstOrderRestriction = voucher.Restrictions.FirstOrDefault(r => r.Type == RestrictionType.IsFirstOrder);
                if (dto.FirstOrderOnly.Value)
                {
                    if (firstOrderRestriction == null)
                    {
                        voucher.Restrictions.Add(new VoucherRestriction
                        {
                            Id = Guid.NewGuid(),
                            VoucherId = voucher.Id,
                            Type = RestrictionType.IsFirstOrder,
                            Value = "true"
                        });
                    }
                }
                else
                {
                    if (firstOrderRestriction != null)
                    {
                        voucher.Restrictions.Remove(firstOrderRestriction);
                    }
                }
            }

            voucher.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher updated. Id: {VoucherId}", voucher.Id);

            return OperationResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher),"Voucher updated successfully");
        }

        public async Task<OperationResult<VoucherDto>> UpdateStatusAsync(Guid id,UpdateVoucherStatusDto dto,CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
            if (voucher == null)
            {
                return OperationResult<VoucherDto>.Failure("Voucher not found");
            }

            voucher.Status = dto.Status;
            voucher.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher status updated. Id: {VoucherId}, Status: {Status}", voucher.Id, dto.Status);

            return OperationResult<VoucherDto>.Success(_mapper.Map<VoucherDto>(voucher),"Voucher status updated successfully");
        }

        public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var voucher = await _voucherRepository.GetByIdAsync(id, cancellationToken);
            if (voucher == null)
            {
                return OperationResult.Failure("Voucher not found");
            }

            if (voucher.Quota != null && voucher.Quota.UsedCount > 0)
            {
                return OperationResult.Failure("Cannot delete a voucher that has been used. Disable it instead.");
            }

            // Set DeletedBy before soft delete
            voucher.DeletedBy = _currentUserService.UserName ?? _currentUserService.Email ?? "System";

            _voucherRepository.Remove(voucher);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher deleted. Id: {VoucherId}", voucher.Id);

            return OperationResult.Success("Voucher deleted successfully");
        }

        // =============================================
        // Customer — Apply Voucher
        // =============================================

        public async Task<OperationResult<ApplyVoucherResponse>> ApplyVoucherAsync(ApplyVoucherRequest request,Guid userId,CancellationToken cancellationToken = default)
        {
            // 1. Find voucher by code
            var voucher = await _voucherRepository.GetByCodeAsync(request.Code.Trim().ToUpper(), cancellationToken);
            if (voucher == null)
            {
                return OperationResult<ApplyVoucherResponse>.Failure("Voucher not found");
            }

            // 2. Find booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult<ApplyVoucherResponse>.Failure("Booking not found");
            }

            // 3. Check if booking already has a voucher
            var existingBv = await _bookingVoucherRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
            if (existingBv != null)
            {
                return OperationResult<ApplyVoucherResponse>.Failure("This booking already has a voucher applied");
            }

            var orderValue = booking.FinalPrice ?? booking.EstimatedPrice ?? 0;

            // 4. Validate voucher (including structural restrictions)
            var validationError = await ValidateVoucherAsync(voucher, userId, orderValue, booking, cancellationToken);
            if (validationError != null)
            {
                await LogUsageAsync(voucher.Id, userId, request.BookingId, 0, false, validationError, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return OperationResult<ApplyVoucherResponse>.Failure(validationError);
            }

            // 5. Calculate discount
            long discountAmount = CalculateDiscount(voucher, orderValue);
            long finalPrice = Math.Max(0, orderValue - discountAmount);

            // 6. Execute in transaction
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Increment usage count
                if (voucher.Quota != null)
                {
                    voucher.Quota.UsedCount++;
                }

                // Increment campaign budget usage
                if (voucher.CampaignId.HasValue && voucher.Campaign != null)
                {
                    voucher.Campaign.BudgetUsed += discountAmount;
                }

                _voucherRepository.Update(voucher);

                // Update booking final price
                booking.FinalPrice = finalPrice;
                _bookingRepository.Update(booking);

                // Create BookingVoucher link
                var bookingVoucher = new BookingVoucher
                {
                    BookingId = request.BookingId,
                    VoucherId = voucher.Id,
                    DiscountAmount = discountAmount,
                    AppliedAt = DateTime.UtcNow,
                };
                await _bookingVoucherRepository.AddAsync(bookingVoucher, cancellationToken);

                // Log successful usage
                await LogUsageAsync(voucher.Id, userId, request.BookingId, discountAmount, true, null, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Failed to apply voucher {VoucherCode} to booking {BookingId}", voucher.Code, request.BookingId);
                return OperationResult<ApplyVoucherResponse>.Failure("Failed to apply voucher. Please try again.");
            }

            _logger.LogInformation("Voucher {VoucherCode} applied to booking {BookingId}. Discount: {Discount}",
                voucher.Code, request.BookingId, discountAmount);

            return OperationResult<ApplyVoucherResponse>.Success(
                new ApplyVoucherResponse
                {
                    Code = voucher.Code,
                    DiscountAmount = discountAmount,
                    OriginalPrice = orderValue,
                    FinalPrice = finalPrice,
                },
                "Voucher applied successfully");
        }

        // =============================================
        // Customer — Release Voucher (on booking cancel)
        // =============================================

        public async Task<OperationResult> ReleaseVoucherAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            var bookingVoucher = await _bookingVoucherRepository.GetByBookingIdAsync(bookingId, cancellationToken);
            if (bookingVoucher == null)
            {
                return OperationResult.Success("No voucher to release");
            }

            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            var voucher = await _voucherRepository.GetByIdAsync(bookingVoucher.VoucherId, cancellationToken);

            if (voucher != null)
            {
                if (voucher.Quota != null && voucher.Quota.UsedCount > 0)
                {
                    voucher.Quota.UsedCount--;
                }

                // Decrement/Refund campaign budget usage
                if (voucher.CampaignId.HasValue && voucher.Campaign != null)
                {
                    voucher.Campaign.BudgetUsed = Math.Max(0, voucher.Campaign.BudgetUsed - bookingVoucher.DiscountAmount);
                }

                _voucherRepository.Update(voucher);
            }

            // Restore original price
            if (booking != null)
            {
                booking.FinalPrice = (booking.FinalPrice ?? 0) + bookingVoucher.DiscountAmount;
                _bookingRepository.Update(booking);
            }

            // Remove the booking voucher link
            _bookingVoucherRepository.Remove(bookingVoucher);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Voucher released from booking {BookingId}. VoucherId: {VoucherId}",
                bookingId, bookingVoucher.VoucherId);

            return OperationResult.Success("Voucher released successfully");
        }

        public async Task<OperationResult<List<EligibleVoucherDto>>> GetEligibleVouchersAsync(
            GetEligibleVouchersRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            // 1. Find booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                return OperationResult<List<EligibleVoucherDto>>.Failure("Booking not found");
            }

            var orderValue = booking.FinalPrice ?? booking.EstimatedPrice ?? 0;

            // 2. Fetch all active vouchers
            var activeVouchers = await _voucherRepository.GetActiveVouchersAsync(cancellationToken);

            var resultList = new List<EligibleVoucherDto>();

            // 3. Evaluate each voucher
            foreach (var voucher in activeVouchers)
            {
                var validationError = await ValidateVoucherAsync(voucher, userId, orderValue, booking, cancellationToken);
                var isEligible = validationError == null;
                
                if (isEligible)
                {
                    long calculatedDiscount = CalculateDiscount(voucher, orderValue);

                    resultList.Add(new EligibleVoucherDto
                    {
                        Id = voucher.Id,
                        Code = voucher.Code,
                        Type = voucher.Type,
                        Value = voucher.Value,
                        MinOrderValue = voucher.MinOrderValue,
                        MaxDiscount = voucher.MaxDiscount,
                        ExpiresAt = voucher.ExpiresAt,
                        Description = voucher.Description ?? string.Empty,
                        IsEligible = true,
                        IneligibleReason = string.Empty,
                        CalculatedDiscount = calculatedDiscount
                    });
                }
            }

            // 4. Sort: Highest discount first, then by code alphabetically
            var sortedResult = resultList
                .OrderByDescending(v => v.CalculatedDiscount)
                .ThenBy(v => v.Code)
                .ToList();

            return OperationResult<List<EligibleVoucherDto>>.Success(sortedResult, "Eligible vouchers retrieved successfully");
        }

        // =============================================
        // Private Helpers
        // =============================================

        private async Task<string?> ValidateVoucherAsync(
            Domain.Entity.Voucher voucher,
            Guid userId,
            long orderValue,
            Domain.Entity.Booking booking,
            CancellationToken cancellationToken)
        {
            // Step 1: Check status
            if (voucher.Status != VoucherStatus.Active)
                return "Voucher is not active";

            var now = DateTime.UtcNow.AddHours(7); // Vietnamese Time (GMT+7)

            // Step 0: Check Campaign rules if voucher belongs to a campaign
            if (voucher.CampaignId.HasValue && voucher.Campaign != null)
            {
                var campaign = voucher.Campaign;

                // 1. Check campaign status
                if (campaign.Status != CampaignStatus.Active)
                {
                    return $"This voucher belongs to a campaign that is currently {campaign.Status.ToString().ToLower()}";
                }

                // 2. Check campaign validity period
                if (now < campaign.StartsAt)
                {
                    return "The promotion campaign has not started yet";
                }
                if (now > campaign.ExpiresAt)
                {
                    return "The promotion campaign has ended";
                }

                // 3. Check campaign budget limit
                if (campaign.BudgetLimit.HasValue)
                {
                    var estimatedDiscount = CalculateDiscount(voucher, orderValue);
                    if ((campaign.BudgetUsed + estimatedDiscount) > campaign.BudgetLimit.Value)
                    {
                        return "The promotion campaign budget has been fully spent";
                    }
                }

                // 4. Check dynamic event trigger
                if (campaign.AutoTriggerEvent.HasValue)
                {
                    switch (campaign.AutoTriggerEvent.Value)
                    {
                        case VoucherEventType.FirstBookingCompleted:
                            var completedCountForFirst = await _bookingRepository.CountAsync(
                                b => b.CustomerProfile != null && b.CustomerProfile.UserId == userId && b.Status == BookingStatus.Completed,
                                cancellationToken);

                            if (completedCountForFirst < 1)
                            {
                                return "This promotion is only available after completing your first booking";
                            }
                            if (completedCountForFirst > 1)
                            {
                                return "This promotion was only available immediately after your first booking completed";
                            }
                            break;
                    }
                }
            }

            // Step 2: Check start date
            if (now < voucher.StartsAt)
                return "Voucher has not started yet";

            // Step 3: Check expiry
            if (now > voucher.ExpiresAt)
                return "Voucher has expired";

            // Step 4: Check system-wide usage limit
            if (voucher.Quota != null && voucher.Quota.MaxUsage.HasValue && voucher.Quota.UsedCount >= voucher.Quota.MaxUsage.Value)
                return "Voucher is out of stock";

            // Step 5: Check minimum order value
            if (orderValue < voucher.MinOrderValue)
                return $"Order value must be at least {voucher.MinOrderValue}";

            // Step 6: Check per-user usage limit
            if (voucher.Quota != null && voucher.Quota.MaxUsagePerUser.HasValue)
            {
                var userUsageCount = await _usageHistoryRepository.GetUsageCountByUserAsync(
                    voucher.Id, userId, cancellationToken);

                if (userUsageCount >= voucher.Quota.MaxUsagePerUser.Value)
                    return "You have already used this voucher the maximum number of times";
            }

            // Step 7: Apply dynamic Voucher Restrictions
            foreach (var restriction in voucher.Restrictions)
            {
                switch (restriction.Type)
                {
                    case RestrictionType.Category:
                        if (Guid.TryParse(restriction.Value, out var allowedCatId))
                        {
                            if (booking.CategoryId != allowedCatId)
                                return "This voucher is not applicable to the selected service category";
                        }
                        break;

                    case RestrictionType.City:
                        if (!string.IsNullOrEmpty(restriction.Value))
                        {
                            if (!booking.Address.Contains(restriction.Value, StringComparison.OrdinalIgnoreCase))
                            {
                                return $"This voucher is only applicable to orders in {restriction.Value}";
                            }
                        }
                        break;

                    case RestrictionType.IsFirstOrder:
                        var hasCompletedBooking = await _bookingRepository.ExistsAsync(
                            b => b.CustomerProfile != null && b.CustomerProfile.UserId == userId && b.Status == BookingStatus.Completed,
                            cancellationToken);

                        if (hasCompletedBooking)
                        {
                            return "This voucher is only applicable to your first order";
                        }
                        break;
                }
            }

            return null; 
        }

        private static long CalculateDiscount(Domain.Entity.Voucher voucher, long orderValue)
        {
            long discount;

            if (voucher.Type == VoucherType.Percent)
            {
                discount = orderValue * voucher.Value / 100;

                // Apply max discount cap
                if (voucher.MaxDiscount.HasValue)
                {
                    discount = Math.Min(discount, voucher.MaxDiscount.Value);
                }
            }
            else // Fixed
            {
                discount = voucher.Value;
            }

            // Discount cannot exceed order value
            return Math.Min(discount, orderValue);
        }

        private async Task LogUsageAsync(Guid voucherId,Guid userId,Guid? bookingId,long discountAmount,bool isSuccess,string? failReason,CancellationToken cancellationToken)
        {
            var history = new VoucherUsageHistory
            {
                VoucherId = voucherId,
                UserId = userId,
                BookingId = bookingId,
                DiscountAmount = discountAmount,
                AppliedAt = DateTime.UtcNow,
                IsSuccess = isSuccess,
                FailReason = failReason,
            };

            await _usageHistoryRepository.AddAsync(history, cancellationToken);
        }
    }
}
