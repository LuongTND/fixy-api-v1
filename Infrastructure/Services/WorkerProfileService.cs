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
using Domain.Exceptions;

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
        private readonly IWorkerWeeklyScheduleService _workerWeeklyScheduleService;

        public WorkerProfileService(
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            IWorkerProfileRepository workerProfileRepository,
            IWorkerServiceRepository workerServiceRepository,
            IWorkerCertificateRepository workerCertificateRepository,
            IWalletRepository walletRepository,
            IMediaRepository mediaRepository,
            IUnitOfWork unitOfWork,
            IBlobService blobService,
            IWorkerWeeklyScheduleService workerWeeklyScheduleService
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
            _workerWeeklyScheduleService = workerWeeklyScheduleService;
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
                    DateOfBirth = i.User?.DateOfBirth,
                    FullName = i.User!.FullName,
                    Gender = i.User?.Gender,
                    Status = i.Status.ToString(),
                    ExperienceYears = i.ExperienceYears,
                    RatingAvg = i.RatingAvg,
                    Services = i.Services.Select(MapWorkerService).ToList(),
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

        public async Task<OperationResult<WorkerPublicDetailDto>> GetPublicDetailAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var data = await GetWorkerProfileDataAsync(workerId, cancellationToken);
            return OperationResult<WorkerPublicDetailDto>.Success(
                new WorkerPublicDetailDto
                {
                    Id = workerId,
                    FullName = data.WorkerProfile.User!.FullName,
                    Bio = data.WorkerProfile.Bio,
                    RatingAvg = data.WorkerProfile.RatingAvg,
                    ExperienceYears = data.WorkerProfile.ExperienceYears,
                    Services = data.WorkerProfile.Services.Select(MapWorkerService).ToList(),
                    Certificates = data
                        .WorkerProfile.Certificates.Select(c =>
                            MapCertificate(c, data.CertificateImageLookup)
                        )
                        .ToList(),

                    PortfolioImages = data.PortfolioImages.Select(MapMedia).ToList(),
                },
                "Get worker profile by Id successfully"
            );
        }

        public async Task<OperationResult<WorkerPrivateDetailDto>> GetPrivateDetailAsync(
            Guid workerId,
            CancellationToken cancellationToken
        )
        {
            var data = await GetWorkerProfileDataAsync(workerId, cancellationToken);

            return OperationResult<WorkerPrivateDetailDto>.Success(
                new WorkerPrivateDetailDto
                {
                    Id = workerId,
                    FullName = data.WorkerProfile.User!.FullName,
                    Email = data.WorkerProfile.User.Email!,
                    Phone = data.WorkerProfile.User.Phone!,
                    RatingAvg = data.WorkerProfile.RatingAvg,
                    Bio = data.WorkerProfile.Bio,
                    ExperienceYears = data.WorkerProfile.ExperienceYears,
                    Services = data.WorkerProfile.Services.Select(MapWorkerService).ToList(),
                    Certificates = data
                        .WorkerProfile.Certificates.Select(c =>
                            MapCertificate(c, data.CertificateImageLookup)
                        )
                        .ToList(),

                    PortfolioImages = data.PortfolioImages.Select(MapMedia).ToList(),
                },
                "Get worker profile by Id successfully"
            );
        }

        public async Task<
            OperationResult<WorkerAdminAndOwnerDetailDto>
        > GetAdminAndOwnerDetailAsync(Guid workerId, CancellationToken cancellationToken)
        {
            var data = await GetWorkerProfileDataAsync(workerId, cancellationToken);
            var dto = new WorkerAdminAndOwnerDetailDto
            {
                Id = data.WorkerProfile.Id,
                FullName = data.WorkerProfile.User!.FullName,
                Email = data.WorkerProfile.User.Email!,
                Phone = data.WorkerProfile.User.Phone!,

                Gender = data.WorkerProfile.User.Gender,
                DateOfBirth = data.WorkerProfile.User.DateOfBirth,

                Status = data.WorkerProfile.Status,

                Bio = data.WorkerProfile.Bio,
                ExperienceYears = data.WorkerProfile.ExperienceYears,
                MaxDistanceKm = data.WorkerProfile.MaxDistanceKm,

                CitizenIdNumber = data.WorkerProfile.User.CitizenIdNumber,
                CitizenIdIssueDate = data.WorkerProfile.User.CitizenIdIssueDate,
                CitizenIdIssuePlace = data.WorkerProfile.User.CitizenIdIssuePlace,

                RejectReason = data.WorkerProfile.RejectReason,

                Services = data.WorkerProfile.Services.Select(MapWorkerService).ToList(),
                Certificates = data
                    .WorkerProfile.Certificates.Select(c =>
                        MapCertificate(c, data.CertificateImageLookup)
                    )
                    .ToList(),

                PortfolioImages = data.PortfolioImages.Select(MapMedia).ToList(),

                IdentificationImages = data.IdentificationImages.Select(MapMedia).ToList(),
            };

            return OperationResult<WorkerAdminAndOwnerDetailDto>.Success(
                dto,
                "Get admin worker detail successfully"
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
            if (dto.CreateAddressRequestDto == null)
            {
                return OperationResult.Failure("Worker need to provice address.");
            }
            if (dto.PortfolioUploads.Count > 10)
            {
                return OperationResult.Failure(
                    "Worker is only allowed to upload a maximum of 10 image portlio."
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
            var existingWorker = await _workerProfileRepository.GetWorkerProfileDetailByUserIdAsync(
                user.Id,
                cancellationToken
            );

            if (existingWorker != null)
            {
                return OperationResult.Failure("User already registered as worker");
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
                // Create Worker Schedule
                await _workerWeeklyScheduleService.CreateDefaultScheduleAsync(
                    workerProfile.Id,
                    cancellationToken
                );
                // Create Worker Address
                var workerAddress = new Address
                {
                    UserId = user.Id,
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
                // Upload Portfolio Images

                foreach (var upload in dto.PortfolioUploads)
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
                await _workerProfileRepository.GetWorkerProfileDetailByUserIdAsync(
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
                        OwnerType = WalletOwnerType.Worker,
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

        //Private method
        private WorkerServiceDto MapWorkerService(WorkerService service)
        {
            return new WorkerServiceDto
            {
                Id = service.Id,
                CategoryId = service.CategoryId,
                CategoryName = service.Category?.Name,
                BasePrice = service.BasePrice,
                IsPrimary = service.IsPrimary,
            };
        }

        private MediaDto MapMedia(Media media)
        {
            return new MediaDto
            {
                Id = media.Id,
                OwnerId = media.OwnerId,
                FileUrl = media.FileUrl,
            };
        }

        private WorkerCertificateDto MapCertificate(
            WorkerCertificate certificate,
            ILookup<Guid, Media> imageLookup
        )
        {
            return new WorkerCertificateDto
            {
                Id = certificate.Id,
                WorkerProfileId = certificate.WorkerProfileId,
                Title = certificate.Title,
                IssuedAt = certificate.IssuedAt,
                IssuedBy = certificate.IssuedBy,

                CertificateImage = imageLookup[certificate.Id].Select(MapMedia).ToList(),
            };
        }

        private async Task<(
            WorkerProfile WorkerProfile,
            List<Media> PortfolioImages,
            List<Media> IdentificationImages,
            ILookup<Guid, Media> CertificateImageLookup
        )> GetWorkerProfileDataAsync(Guid workerId, CancellationToken cancellationToken)
        {
            var workerProfile = await _workerProfileRepository.GetWorkerProfileDetailByUserIdAsync(
                workerId,
                cancellationToken
            );

            if (workerProfile == null)
            {
                throw new NotFoundException("Worker profile not found");
            }
            if (workerProfile.User == null)
            {
                throw new NotFoundException("Worker user not found");
            }
            var portfolioImages = await _mediaRepository.GetPorfolioImagesByUserId(
                workerProfile.User.Id,
                cancellationToken
            );

            var identificationImages = await _mediaRepository.GetIdentificateImagesByUserId(
                workerProfile.User.Id,
                cancellationToken
            );

            var certificateIds = workerProfile.Certificates.Select(x => x.Id).ToList();

            var workerCertificateImages =
                await _mediaRepository.GetAllWorkerCertificateImagesByCertificateIds(
                    certificateIds,
                    cancellationToken
                );

            return (
                workerProfile,
                portfolioImages,
                identificationImages,
                workerCertificateImages.ToLookup(x => x.OwnerId)
            );
        }
    }
}
