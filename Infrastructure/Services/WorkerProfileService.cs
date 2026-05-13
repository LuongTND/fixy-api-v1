using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Media;
using Application.DTOs.WorkerProfile;
using Application.DTOs.WorkerProfile.WorkerCertificate;
using Application.DTOs.WorkerProfile.WorkerService;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Services.Auth;
using Domain.Entity;
using Domain.Enum;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Services
{
    public class WorkerProfileService : IWorkerProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IWorkerProfileRepository _workerProfileRepository;
        private readonly IWorkerServiceRepository _workerServiceRepository;
        private readonly IWorkerCertificateRepository _workerCertificateRepository;
        private readonly IMediaRepository _mediaRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPasswordHasher _passwordHasher;

        public WorkerProfileService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IUserOtpRepository userOtpRepository,
            IWorkerProfileRepository workerProfileRepository,
            IWorkerServiceRepository workerServiceRepository,
            IWorkerCertificateRepository workerCertificateRepository,
            IMediaRepository mediaRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher
        )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userOtpRepository = userOtpRepository;
            _workerProfileRepository = workerProfileRepository;
            _workerServiceRepository = workerServiceRepository;
            _workerCertificateRepository = workerCertificateRepository;
            _mediaRepository = mediaRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<
            OperationResult<PagedResponse<WorkerProfileDto>>
        > GetPagedWorkerRegisterRequest(PagedQuery query, CancellationToken cancellationToken)
        {
            var (items, totalCount) =
                await _workerProfileRepository.GetPagedWorkerRegisterRequestAsync(
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
                    Status = i.Status,
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

        public async Task<OperationResult<WorkerProfileDetailDto>> GetWorkerProfileDetailRequest(
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
                                    FilePublicId = x.FilePublicId,
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
                            FilePublicId = x.FilePublicId,
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
            var otp = await _userOtpRepository.GetVerifiedOtpAsync(dto.Target, cancellationToken);

            if (otp == null)
                return OperationResult.Failure("OTP is not verified");
            var existedUser = await _userRepository.GetByTargetAsync(dto.Target, cancellationToken);

            if (existedUser != null)
                return OperationResult.Failure("Account already exists");
            if (dto.WorkerService.Count is < 1 or > 5)
            {
                return OperationResult.Failure(
                    "Worker is only allowed to perform a maximum of 5 services and a minimum of 1 service."
                );
            }
            if (dto.IdentificationUploads.Count != 2)
            {
                return OperationResult.Failure("Workers need to update front and back of ID card");
            }
            if (dto.WorkerService.Count(x => x.IsPrimary) != 1)
            {
                return OperationResult.Failure("Worker must have exactly one primary service");
            }
            //Create Base User
            var user = new User
            {
                FullName = dto.FullName,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                CitizenIdNumber = dto.CitizenIdNumber,
                CitizenIdIssueDate = dto.CitizenIdIssueDate,
                CitizenIdIssuePlace = dto.CitizenIdIssuePlace,
            };
            if (IsPhone(dto.Target))
            {
                user.Phone = dto.Target;
                user.IsPhoneVerified = true;
            }
            else if (IsEmail(dto.Target))
            {
                user.Email = dto.Target;
                user.IsEmailVerified = true;
            }
            else
            {
                return OperationResult.Failure("Invalid email or phone number");
            }

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var identificateImg in dto.IdentificationUploads)
            {
                var identificateImage = new Media
                {
                    OwnerId = user.Id,
                    UploadedById = user.Id,
                    OwnerType = MediaOwnerType.User,
                    Category = MediaCategory.Identificate,
                    FileUrl = identificateImg.FileUrl,
                    FilePublicId = identificateImg.FilePublicId,
                };
                await _mediaRepository.AddAsync(identificateImage, cancellationToken);
            }
            var role = await _roleRepository.GetWorkerRoleAsync(cancellationToken);

            await _userRoleRepository.AddAsync(
                new UserRole { User = user, RoleId = role.Id },
                cancellationToken
            );
            //Create Worker Profile
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
            //Create Worker Service

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

            //Create Worker Certificate
            foreach (var certificate in dto.CerificateUploads)
            {
                var workerCertificate = new WorkerCertificate
                {
                    WorkerProfileId = workerProfile.Id,
                    Title = certificate.Title,
                    IssuedBy = certificate.IssuedBy,
                    IssuedAt = certificate.IssuedAt,
                };
                await _workerCertificateRepository.AddAsync(workerCertificate, cancellationToken);
                foreach (var media in certificate.MediaUploads)
                {
                    var mediaUp = new Media
                    {
                        OwnerId = workerCertificate.Id,
                        UploadedById = user.Id,
                        OwnerType = MediaOwnerType.Certificate,
                        Category = MediaCategory.Certificate,
                        FileUrl = media.FileUrl,
                        FilePublicId = media.FilePublicId,
                    };
                    await _mediaRepository.AddAsync(mediaUp, cancellationToken);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Worker register successfully");
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
                workerRegisterRequest.User.CitizenIdVerifiedAt = DateTime.Now;
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

        private bool IsEmail(string target)
        {
            return Regex.IsMatch(target, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool IsPhone(string target)
        {
            return Regex.IsMatch(target, @"^\d{9,11}$");
        }
    }
}
