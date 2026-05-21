using Application.Common;
using Application.DTOs.Address;
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
                    UserId = i.UserId,
                    FullName = i.User!.FullName,
                    DateOfBirth = i.User.DateOfBirth,
                    Gender = i.User.Gender.ToString(),
                    Status = i.Status.ToString(),
                    ExperienceYears = i.ExperienceYears,
                    RatingAvg = i.RatingAvg,
                    TotalReviews = i.TotalReviews,
                    TotalOrders = i.TotalOrders,

                    Services = i.Services.Select(MapWorkerService).ToList(),
                })
                .ToList();

            return OperationResult<PagedResponse<WorkerProfileDto>>.Success(
                new PagedResponse<WorkerProfileDto>
                {
                    Items = dtoItems,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize,
                    TotalCount = totalCount,
                },
                "Get worker profiles successfully"
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
                    Id = data.WorkerProfile.Id,
                    UserId = data.WorkerProfile.UserId,

                    FullName = data.WorkerProfile.User!.FullName,

                    Bio = data.WorkerProfile.Bio,

                    ExperienceYears = data.WorkerProfile.ExperienceYears,

                    RatingAvg = data.WorkerProfile.RatingAvg,

                    TotalReviews = data.WorkerProfile.TotalReviews,

                    TotalOrders = data.WorkerProfile.TotalOrders,

                    Services = data.WorkerProfile.Services.Select(MapWorkerService).ToList(),

                    Certificates = data
                        .WorkerProfile.Certificates.Select(x =>
                            MapCertificate(x, data.CertificateImageLookup)
                        )
                        .ToList(),

                    PortfolioImages = data.PortfolioImages.Select(MapMedia).ToList(),
                },
                "Get worker public detail successfully"
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
                    Id = data.WorkerProfile.Id,
                    UserId = data.WorkerProfile.UserId,

                    FullName = data.WorkerProfile.User!.FullName,

                    Email = data.WorkerProfile.User.Email!,

                    Phone = data.WorkerProfile.User.Phone!,

                    Bio = data.WorkerProfile.Bio,

                    ExperienceYears = data.WorkerProfile.ExperienceYears,

                    RatingAvg = data.WorkerProfile.RatingAvg,

                    TotalReviews = data.WorkerProfile.TotalReviews,

                    TotalOrders = data.WorkerProfile.TotalOrders,

                    Services = data.WorkerProfile.Services.Select(MapWorkerService).ToList(),

                    Certificates = data
                        .WorkerProfile.Certificates.Select(x =>
                            MapCertificate(x, data.CertificateImageLookup)
                        )
                        .ToList(),

                    PortfolioImages = data.PortfolioImages.Select(MapMedia).ToList(),
                },
                "Get worker private detail successfully"
            );
        }

        public async Task<
            OperationResult<WorkerAdminAndOwnerDetailDto>
        > GetAdminAndOwnerDetailAsync(Guid workerId, CancellationToken cancellationToken)
        {
            var data = await GetWorkerProfileDataAsync(workerId, cancellationToken);

            var address = data.WorkerProfile.Address;

            return OperationResult<WorkerAdminAndOwnerDetailDto>.Success(
                new WorkerAdminAndOwnerDetailDto
                {
                    Id = data.WorkerProfile.Id,
                    UserId = data.WorkerProfile.UserId,

                    FullName = data.WorkerProfile.User!.FullName,

                    Email = data.WorkerProfile.User.Email!,

                    Phone = data.WorkerProfile.User.Phone!,

                    Gender = data.WorkerProfile.User.Gender.ToString(),

                    DateOfBirth = data.WorkerProfile.User.DateOfBirth,

                    Status = data.WorkerProfile.Status,

                    Bio = data.WorkerProfile.Bio,

                    ExperienceYears = data.WorkerProfile.ExperienceYears,

                    MaxDistanceKm = data.WorkerProfile.MaxDistanceKm,

                    RatingAvg = data.WorkerProfile.RatingAvg,

                    TotalReviews = data.WorkerProfile.TotalReviews,

                    TotalOrders = data.WorkerProfile.TotalOrders,

                    CitizenIdNumber = data.WorkerProfile.User.CitizenIdNumber,

                    CitizenIdIssueDate = data.WorkerProfile.User.CitizenIdIssueDate,

                    CitizenIdIssuePlace = data.WorkerProfile.User.CitizenIdIssuePlace,

                    RejectReason = data.WorkerProfile.RejectReason,

                    Address =
                        address == null
                            ? null
                            : new AddressDto
                            {
                                Id = address.Id,
                                City = address.City,
                                District = address.District,
                                Ward = address.Ward,
                                Detail = address.Detail,
                                Lat = address.Lat,
                                Lng = address.Lng,
                            },

                    Services = data.WorkerProfile.Services.Select(MapWorkerService).ToList(),

                    Certificates = data
                        .WorkerProfile.Certificates.Select(x =>
                            MapCertificate(x, data.CertificateImageLookup)
                        )
                        .ToList(),

                    PortfolioImages = data.PortfolioImages.Select(MapMedia).ToList(),

                    IdentificationImages = data.IdentificationImages.Select(MapMedia).ToList(),
                },
                "Get worker detail successfully"
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
                    TotalReviews = 0,
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
                    WorkerProfileId = workerProfile.Id,
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
            var workerRegisterRequest = await _workerProfileRepository.GetByIdAsync(
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
                var existingWallet = await _walletRepository.GetByUserIdAsync(
                    workerRegisterRequest.User.Id,
                    WalletOwnerType.Worker,
                    cancellationToken
                );

                if (existingWallet == null)
                {
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

        public async Task<OperationResult> UpdateWorkerProfileAsync(
            Guid workerId,
            WorkerProfileUpdateRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            if (dto.Services.Count is < 1 or > 5)
            {
                return OperationResult.Failure(
                    "Worker is only allowed to perform a maximum of 5 services and a minimum of 1 service."
                );
            }

            if (dto.Services.Count(x => x.IsPrimary) != 1)
            {
                return OperationResult.Failure("Worker must have exactly one primary service.");
            }

            var workerProfile = await _workerProfileRepository.GetWorkerProfileDetailByUserIdAsync(
                workerId,
                cancellationToken
            );

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            var user = workerProfile.User;

            if (user == null)
            {
                return OperationResult.Failure("User not found");
            }

            // =========================
            // Update User
            // =========================

            user.Phone = dto.Phone;

            _userRepository.Update(user);

            // =========================
            // Update Worker Profile
            // =========================

            workerProfile.Bio = dto.Bio;
            workerProfile.ExperienceYears = dto.ExperienceYears;
            workerProfile.MaxDistanceKm = dto.MaxDistanceKm;
            workerProfile.RejectReason = null;
            // pending lại cần admin duyệt lại
            workerProfile.Status = WorkerStatus.Pending;

            _workerProfileRepository.Update(workerProfile);

            // =========================
            // Update Address
            // =========================

            var address = await _addressRepository.GetWorkerAddressAsync(
                workerProfile.Id,
                cancellationToken
            );

            if (address == null)
            {
                return OperationResult.Failure("Address not found");
            }

            address.City = dto.Address.City;
            address.District = dto.Address.District;
            address.Ward = dto.Address.Ward;
            address.Detail = dto.Address.Detail;
            address.Lat = dto.Address.Lat;
            address.Lng = dto.Address.Lng;

            _addressRepository.Update(address);

            // =========================
            // Replace Services
            // =========================

            _workerServiceRepository.RemoveRange(workerProfile.Services);

            var newServices = dto.Services.Select(x => new WorkerService
            {
                WorkerProfileId = workerProfile.Id,
                CategoryId = x.CategoryId,
                BasePrice = x.BasePrice,
                IsPrimary = x.IsPrimary,
            });
            if (dto.Avatar != null)
            {
                string? newAvatarUrl = null;

                try
                {
                    // upload new avatar
                    newAvatarUrl = await _blobService.UploadImageAsync(dto.Avatar);

                    // get old avatar media
                    var oldAvatarMedia = await _mediaRepository.GetAvatarByUserIdAsync(
                        user.Id,
                        cancellationToken
                    );

                    // delete old blob
                    if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                    {
                        await _blobService.DeleteImageAsync(user.AvatarUrl);
                    }

                    // remove old media
                    if (oldAvatarMedia != null)
                    {
                        _mediaRepository.Remove(oldAvatarMedia);
                    }

                    // add new media
                    await _mediaRepository.AddAsync(
                        new Media
                        {
                            OwnerId = user.Id,
                            UploadedById = user.Id,
                            OwnerType = MediaOwnerType.User,
                            Category = MediaCategory.Avatar,
                            FileUrl = newAvatarUrl,
                        },
                        cancellationToken
                    );

                    // update user avatar
                    user.AvatarUrl = newAvatarUrl;
                }
                catch
                {
                    if (!string.IsNullOrWhiteSpace(newAvatarUrl))
                    {
                        await _blobService.DeleteImageAsync(newAvatarUrl);
                    }

                    throw;
                }
            }

            await _workerServiceRepository.AddRangeAsync(newServices, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Update worker profile successfully");
        }

        public async Task<OperationResult> UploadPortfolioImagesAsync(
            Guid workerId,
            UploadPortfolioImagesRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            if (dto.Images.Count == 0)
            {
                return OperationResult.Failure("Please upload at least one image.");
            }

            var currentImages = await _mediaRepository.GetPorfolioImagesByUserId(
                workerId,
                cancellationToken
            );

            if (currentImages.Count + dto.Images.Count > 10)
            {
                return OperationResult.Failure("Maximum 10 portfolio images allowed.");
            }

            var uploadedUrls = new List<string>();

            try
            {
                foreach (var image in dto.Images)
                {
                    var imageUrl = await _blobService.UploadImageAsync(image);

                    uploadedUrls.Add(imageUrl);

                    var media = new Media
                    {
                        OwnerId = workerId,
                        UploadedById = workerId,
                        OwnerType = MediaOwnerType.WorkerProfile,
                        Category = MediaCategory.Portfolio,
                        FileUrl = imageUrl,
                    };

                    await _mediaRepository.AddAsync(media, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Upload portfolio images successfully.");
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

        public async Task<OperationResult> DeletePortfolioImageAsync(
            Guid workerId,
            Guid mediaId,
            CancellationToken cancellationToken
        )
        {
            var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);

            if (media == null)
            {
                return OperationResult.Failure("Image not found");
            }

            if (
                media.OwnerId != workerId
                || media.Category != MediaCategory.Portfolio
                || media.OwnerType != MediaOwnerType.WorkerProfile
            )
            {
                return OperationResult.Failure("Forbidden");
            }

            _mediaRepository.Remove(media);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _blobService.DeleteImageAsync(media.FileUrl);

            return OperationResult.Success("Delete portfolio image successfully.");
        }

        public async Task<OperationResult> UpdateIdentificationAsync(
            Guid workerId,
            UpdateIdentificationRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var workerProfile = await _workerProfileRepository.GetWorkerProfileByUserIdAsync(
                workerId,
                cancellationToken
            );

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker register request not found");
            }
            if (dto.Images.Count != 2)
            {
                return OperationResult.Failure(
                    "Identification must include front and back images."
                );
            }
            workerProfile.Status = WorkerStatus.Pending;
            if (workerProfile.User != null)
            {
                workerProfile.User.CitizenIdNumber = dto.CitizenIdNumber;
                workerProfile.User.CitizenIdIssuePlace = dto.CitizenIdIssuePlace;
                workerProfile.User.CitizenIdIssueDate = dto.CitizenIdIssueDate;
            }
            var currentImages = await _mediaRepository.GetIdentificateImagesByUserId(
                workerId,
                cancellationToken
            );

            var uploadedUrls = new List<string>();

            try
            {
                var newMedias = new List<Media>();

                foreach (var image in dto.Images)
                {
                    var imageUrl = await _blobService.UploadImageAsync(image);

                    uploadedUrls.Add(imageUrl);

                    newMedias.Add(
                        new Media
                        {
                            OwnerId = workerId,
                            UploadedById = workerId,
                            OwnerType = MediaOwnerType.User,
                            Category = MediaCategory.Identification,
                            FileUrl = imageUrl,
                        }
                    );
                }

                // delete old blob
                foreach (var oldImage in currentImages)
                {
                    await _blobService.DeleteImageAsync(oldImage.FileUrl);
                }

                // remove old db
                foreach (var oldImage in currentImages)
                {
                    _mediaRepository.Remove(oldImage);
                }

                // add new db
                foreach (var media in newMedias)
                {
                    await _mediaRepository.AddAsync(media, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Update identification images successfully.");
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

        public async Task<OperationResult> UpdateCentificatesAsync(
            Guid workerId,
            List<WorkerCertificateUploadRequestDto> dto,
            CancellationToken cancellationToken
        )
        {
            var workerProfile = await _workerProfileRepository.GetWorkerProfileDetailByUserIdAsync(
                workerId,
                cancellationToken
            );

            if (workerProfile == null)
            {
                return OperationResult.Failure("Worker profile not found");
            }

            var uploadedUrls = new List<string>();

            try
            {
                var oldCertificates = workerProfile.Certificates.ToList();

                var oldCertificateIds = oldCertificates.Select(x => x.Id).ToList();

                var oldCertificateImages =
                    await _mediaRepository.GetAllWorkerCertificateImagesByCertificateIds(
                        oldCertificateIds,
                        cancellationToken
                    );

                foreach (var image in oldCertificateImages)
                {
                    await _blobService.DeleteImageAsync(image.FileUrl);
                }

                foreach (var image in oldCertificateImages)
                {
                    _mediaRepository.Remove(image);
                }

                _workerCertificateRepository.RemoveRange(oldCertificates);

                foreach (var certificate in dto)
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

                    // Upload certificate images

                    foreach (var upload in certificate.MediaUploads)
                    {
                        var imageUrl = await _blobService.UploadImageAsync(upload);

                        uploadedUrls.Add(imageUrl);

                        var media = new Media
                        {
                            OwnerId = workerCertificate.Id,
                            UploadedById = workerId,
                            OwnerType = MediaOwnerType.Certificate,
                            Category = MediaCategory.Certificate,
                            FileUrl = imageUrl,
                        };

                        await _mediaRepository.AddAsync(media, cancellationToken);
                    }
                }

                // pending lại để admin duyệt lại
                workerProfile.Status = WorkerStatus.Pending;

                _workerProfileRepository.Update(workerProfile);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult.Success("Update certificates successfully");
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

        //Private method
        private WorkerServiceDto MapWorkerService(WorkerService service)
        {
            return new WorkerServiceDto
            {
                Id = service.Id,
                WorkerProfileId = service.WorkerProfileId,
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
