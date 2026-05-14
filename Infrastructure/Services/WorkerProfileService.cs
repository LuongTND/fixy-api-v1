using Application.Common;
using Application.DTOs.Media;
using Application.DTOs.WorkerProfile;
using Application.DTOs.WorkerProfile.WorkerCertificate;
using Application.DTOs.WorkerProfile.WorkerService;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Media;
using Domain.Entity;
using Domain.Enum;

namespace Infrastructure.Services
{
    public class WorkerProfileService : IWorkerProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IWorkerServiceRepository _workerServiceRepository;
        private readonly IWorkerCertificateRepository _workerCertificateRepository;
        private readonly IWalletRepository _walletRepository;

        private readonly IMediaRepository _mediaRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IBlobService _blobService;

        public WorkerProfileService(
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            IWorkerProfileRepository workerProfileRepository,
            IWorkerServiceRepository workerServiceRepository,
            IWorkerCertificateRepository workerCertificateRepository,
            IWalletRepository walletRepository,
            IMediaRepository mediaRepository,
            IUnitOfWork unitOfWork,
            IBlobService blobService
        )
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _workerProfileRepository = workerProfileRepository;
            _workerServiceRepository = workerServiceRepository;
            _workerCertificateRepository = workerCertificateRepository;
            _walletRepository = walletRepository;
            _mediaRepository = mediaRepository;
            _unitOfWork = unitOfWork;
            _blobService = blobService;
        }

        public async Task<OperationResult<PagedResponse<WorkerProfileDto>>> GetPagedWorkerProfiles(
            WorkerProfileQuery query,
            string? role,
            CancellationToken cancellationToken
        )
        {
            if (role != "ADMIN")
            {
                query.Status = WorkerStatus.Approved;
            }
            var (items, totalCount) = await _workerProfileRepository.GetWorkerProfilesAsync(
                query,
                cancellationToken
            );
            var dtoItems = items
                .Select(i => new WorkerProfileDto
                {
                    Id = i.Id,
                    Email = i.User?.Email,
                    Phone = i.User?.Phone,
                    DateOfBirth = i.User?.DateOfBirth,
                    FullName = i.User!.FullName,
                    Gender = i.User?.Gender,
                    Status = i.Status.ToString(),
                    Service = i
                        .Services.Select(s => new WorkerServiceDto
                        {
                            Id = s.Id,
                            CategoryId = s.CategoryId,
                            CategoryName = s.Category?.Name,
                            BasePrice = s.BasePrice,
                            IsPrimary = s.IsPrimary,
                        })
                        .ToList(),
                })
                .ToList();
            return OperationResult<PagedResponse<WorkerProfileDto>>.Success(
                new PagedResponse<WorkerProfileDto>
                {
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                    Items = dtoItems,
                },
                "Get worker profile paged successfully"
            );
        }

        public async Task<OperationResult<WorkerProfileDetailDto>> GetWorkerProfileDetail(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var workerProfile = await _workerProfileRepository.GetWorkerProfileDetailByIdAsync(
                id,
                cancellationToken
            );
            if (workerProfile == null)
            {
                return OperationResult<WorkerProfileDetailDto>.Failure("Worker profile not found");
            }

            var identificateImages = await _mediaRepository.GetIdentificateImagesByUserId(
                workerProfile.User!.Id,
                cancellationToken
            );
            var profolioImages = await _mediaRepository.GetIdentificateImagesByUserId(
                workerProfile.User!.Id,
                cancellationToken
            );
            var certificateIds = workerProfile.Certificates.Select(x => x.Id).ToList();

            var workerCertificateImages =
                await _mediaRepository.GetAllWorkerCertificateImagesByCertificateIds(
                    certificateIds,
                    cancellationToken
                );
            var imageLookup = workerCertificateImages.ToLookup(x => x.OwnerId);
            return OperationResult<WorkerProfileDetailDto>.Success(
                new WorkerProfileDetailDto
                {
                    Id = id,
                    FullName = workerProfile.User!.FullName,
                    Gender = workerProfile.User.Gender,
                    Email = workerProfile.User.Email,
                    Phone = workerProfile.User.Phone,
                    DateOfBirth = workerProfile.User.DateOfBirth,
                    Bio = workerProfile.Bio,
                    ExperienceYears = workerProfile.ExperienceYears,
                    MaxDistanceKm = workerProfile.MaxDistanceKm,
                    Status = workerProfile.Status,
                    CitizenIdNumber = workerProfile.User.CitizenIdNumber,
                    CitizenIdIssuePlace = workerProfile.User.CitizenIdIssuePlace,
                    CitizenIdIssueDate = workerProfile.User.CitizenIdIssueDate,
                    Services = workerProfile
                        .Services.Select(s => new WorkerServiceDto
                        {
                            Id = s.Id,
                            CategoryId = s.CategoryId,
                            CategoryName = s.Category?.Name,
                            BasePrice = s.BasePrice,
                            IsPrimary = s.IsPrimary,
                        })
                        .ToList(),
                    Certificates = workerProfile
                        .Certificates.Select(c => new WorkerCertificateDto
                        {
                            Id = c.Id,
                            WorkerProfileId = c.WorkerProfileId,
                            Title = c.Title,
                            IssuedAt = c.IssuedAt,
                            IssuedBy = c.IssuedBy,

                            CertificateImage = imageLookup[c.Id]
                                .Select(x => new MediaDto
                                {
                                    Id = x.Id,
                                    OwnerId = x.OwnerId,
                                    FileUrl = x.FileUrl,
                                })
                                .ToList(),
                        })
                        .ToList(),
                    IdentificateImages = identificateImages
                        .Select(x => new MediaDto
                        {
                            Id = x.Id,
                            OwnerId = x.OwnerId,
                            FileUrl = x.FileUrl,
                        })
                        .ToList(),
                    ProfolioImages = profolioImages
                        .Select(x => new MediaDto
                        {
                            Id = x.Id,
                            OwnerId = x.OwnerId,
                            FileUrl = x.FileUrl,
                        })
                        .ToList(),
                },
                "Get worker profile by Id successfully"
            );
        }

        public async Task<OperationResult> WorkerRegisterAsync(
            WorkerRegisterRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            if (dto.WorkerService.Count is < 1 or > 5)
            {
                return OperationResult.Failure(
                    "Worker is only allowed to perform a maximum of 5 services and a minimum of 1 service."
                );
            }

            if (dto.IdentificationUploads.Count != 2)
            {
                return OperationResult.Failure("Workers need to upload front and back of ID card");
            }

            if (dto.WorkerService.Count(x => x.IsPrimary) != 1)
            {
                return OperationResult.Failure("Worker must have exactly one primary service");
            }

            var user = await _userRepository.GetByTargetAsync(dto.Target);

            if (user == null)
            {
                return OperationResult.Failure("User not found");
            }

            var uploadedUrls = new List<string>();

            try
            {
                // Create Worker Profile

                var workerProfile = new WorkerProfile
                {
                    UserId = user.Id,
                    Bio = dto.Bio,
                    ExperienceYears = dto.ExperienceYears,
                    MaxDistanceKm = dto.MaxDistanceKm,
                    Status = WorkerStatus.Pending,
                    Badge = WorkerBadge.New,
                    RatingAvg = 0,
                    TotalOrders = 0,
                    IsOnline = false,
                };

                await _workerProfileRepository.AddAsync(workerProfile, cancellationToken);

                // Create Worker Address
                var workerAddress = new Address
                {
                    City = dto.CreateAddressRequestDto.City,
                    District = dto.CreateAddressRequestDto.District,
                    Ward = dto.CreateAddressRequestDto.Ward,
                    Detail = dto.CreateAddressRequestDto.Detail,
                    Lat = dto.CreateAddressRequestDto.Lat,
                    Lng = dto.CreateAddressRequestDto.Lng,
                    IsDefault = true,
                };
                await _addressRepository.AddAsync(workerAddress, cancellationToken);
                // Create Worker Services

                foreach (var service in dto.WorkerService)
                {
                    var workerService = new WorkerService
                    {
                        WorkerProfileId = workerProfile.Id,
                        CategoryId = service.CategoryId,
                        BasePrice = service.BasePrice,
                        IsPrimary = service.IsPrimary,
                    };

                    await _workerServiceRepository.AddAsync(workerService, cancellationToken);
                }
                // Upload Profolio Images

                foreach (var upload in dto.ProfolioUploads)
                {
                    var imageUrl = await _blobService.UploadImageAsync(upload);

                    uploadedUrls.Add(imageUrl);

                    var media = new Media
                    {
                        OwnerId = user.Id,
                        UploadedById = user.Id,
                        OwnerType = MediaOwnerType.WorkerProfile,
                        Category = MediaCategory.Portfolio,
                        FileUrl = imageUrl,
                    };

                    await _mediaRepository.AddAsync(media, cancellationToken);
                }
                // Upload Identification Images

                foreach (var upload in dto.IdentificationUploads)
                {
                    var imageUrl = await _blobService.UploadImageAsync(upload);

                    uploadedUrls.Add(imageUrl);

                    var media = new Media
                    {
                        OwnerId = user.Id,
                        UploadedById = user.Id,
                        OwnerType = MediaOwnerType.User,
                        Category = MediaCategory.Identification,
                        FileUrl = imageUrl,
                    };

                    await _mediaRepository.AddAsync(media, cancellationToken);
                }

                // Create Certificates

                foreach (var certificate in dto.CertificateUploads)
                {
                    var workerCertificate = new WorkerCertificate
                    {
                        WorkerProfileId = workerProfile.Id,
                        Title = certificate.Title,
                        IssuedBy = certificate.IssuedBy,
                        IssuedAt = certificate.IssuedAt,
                    };

                    await _workerCertificateRepository.AddAsync(
                        workerCertificate,
                        cancellationToken
                    );

                    // Upload Certificate Images

                    foreach (var upload in certificate.MediaUploads)
                    {
                        var imageUrl = await _blobService.UploadImageAsync(upload);

                        uploadedUrls.Add(imageUrl);

                        var media = new Media
                        {
                            OwnerId = workerCertificate.Id,
                            UploadedById = user.Id,
                            OwnerType = MediaOwnerType.Certificate,
                            Category = MediaCategory.Certificate,
                            FileUrl = imageUrl,
                        };

                        await _mediaRepository.AddAsync(media, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Worker register successfully");
            }
            catch
            {
                foreach (var url in uploadedUrls)
                {
                    await _blobService.DeleteImageAsync(url);
                }

                throw;
            }
        }

        public async Task<OperationResult> ApproveWorkerRegisterRequest(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var workerRegisterRequest =
                await _workerProfileRepository.GetWorkerProfileDetailByIdAsync(
                    id,
                    cancellationToken
                );
            if (workerRegisterRequest == null)
            {
                return OperationResult.Failure("Worker register request not found");
            }
            workerRegisterRequest.Status = WorkerStatus.Approved;
            workerRegisterRequest.ApprovedById = userId;
            if (workerRegisterRequest.User != null)
            {
                workerRegisterRequest.User.IsCitizenIdVerified = true;
                await _walletRepository.AddAsync(
                    new Wallet
                    {
                        UserId = workerRegisterRequest.User.Id,
                        OwnerType = WalletOwnerType.Customer,
                        Balance = 0,
                        LifetimeEarned = 0,
                        LifetimeSpent = 0,
                        CreatedAt = DateTime.UtcNow,
                    },
                    cancellationToken
                );
            }

            _workerProfileRepository.Update(workerRegisterRequest);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Worker register was approved successfully");
        }

        public async Task<OperationResult> RejectWorkerRegisterRequest(
            Guid id,
            string reason,
            CancellationToken cancellationToken
        )
        {
            var workerRegisterRequest = await _workerProfileRepository.GetByIdAsync(
                id,
                cancellationToken
            );
            if (workerRegisterRequest == null)
            {
                return OperationResult.Failure("Worker register request not found");
            }
            workerRegisterRequest.Status = WorkerStatus.Rejected;
            workerRegisterRequest.RejectReason = reason;
            _workerProfileRepository.Update(workerRegisterRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Worker register was approved successfully");
        }
    }
}
