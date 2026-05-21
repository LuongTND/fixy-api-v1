using Application.Common;
using Application.DTOs.Address;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entity;

namespace Infrastructure.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddressService(
            IAddressRepository addressRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork
        )
        {
            _addressRepository = addressRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OperationResult<IEnumerable<AddressDto>>> GetAddressByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByIdWithProfilesAsync(userId, cancellationToken);

            if (user == null)
            {
                return OperationResult<IEnumerable<AddressDto>>.Failure("User not found");
            }

            var roles = user.UserRoles.Select(x => x.Role!.Name).ToList();

            var result = new List<Address>();

            // =========================
            // Customer Addresses
            // =========================

            if (roles.Contains("CUSTOMER") && user.CustomerProfile != null)
            {
                var customerAddresses = await _addressRepository.GetByCustomerProfileIdAsync(
                    user.CustomerProfile.Id,
                    cancellationToken
                );

                result.AddRange(customerAddresses);
            }

            // =========================
            // Worker Address
            // =========================

            if (roles.Contains("WORKER") && user.WorkerProfile != null)
            {
                var workerAddress = await _addressRepository.GetWorkerAddressAsync(
                    user.WorkerProfile.Id,
                    cancellationToken
                );

                if (workerAddress != null)
                {
                    result.Add(workerAddress);
                }
            }

            return OperationResult<IEnumerable<AddressDto>>.Success(
                result.Select(address => new AddressDto
                {
                    Id = address.Id,
                    Label = address.Label,
                    City = address.City,
                    District = address.District,
                    Ward = address.Ward,
                    Detail = address.Detail,
                    Lat = address.Lat,
                    Lng = address.Lng,
                    IsDefault = address.IsDefault,
                    CreatedDate = address.CreatedDate,
                }),
                "Get addresses successfully"
            );
        }

        public async Task<OperationResult<AddressDto>> CreateAsync(
            Guid userId,
            CreateAddressRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByIdWithProfilesAsync(userId, cancellationToken);

            if (user == null)
            {
                return OperationResult<AddressDto>.Failure("User not found");
            }

            var roles = user.UserRoles.Select(x => x.Role!.Name).ToList();

            var isCustomer = roles.Contains("CUSTOMER");

            // =========================
            // Worker
            // =========================

            // Worker address only create during register

            // =========================
            // Customer
            // =========================

            if (isCustomer)
            {
                if (user.CustomerProfile == null)
                {
                    return OperationResult<AddressDto>.Failure("Customer profile not found");
                }

                var customerProfileId = user.CustomerProfile.Id;

                var hasAnyAddress = await _addressRepository.ExistsAsync(
                    x => x.CustomerProfileId == customerProfileId && !x.IsDeleted,
                    cancellationToken
                );

                if (dto.IsDefault)
                {
                    var currentDefault =
                        await _addressRepository.GetDefaultByCustomerProfileIdAsync(
                            customerProfileId,
                            cancellationToken
                        );

                    if (currentDefault != null)
                    {
                        currentDefault.IsDefault = false;

                        _addressRepository.Update(currentDefault);
                    }
                }

                var address = new Address
                {
                    CustomerProfileId = customerProfileId,
                    Label = dto.Label,
                    City = dto.City,
                    District = dto.District,
                    Ward = dto.Ward,
                    Detail = dto.Detail,
                    Lat = dto.Lat,
                    Lng = dto.Lng,
                    IsDefault = !hasAnyAddress || dto.IsDefault,
                };

                await _addressRepository.AddAsync(address, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return OperationResult<AddressDto>.Success(
                    new AddressDto
                    {
                        Id = address.Id,
                        Label = address.Label,
                        City = address.City,
                        District = address.District,
                        Ward = address.Ward,
                        Detail = address.Detail,
                        Lat = address.Lat,
                        Lng = address.Lng,
                        IsDefault = address.IsDefault,
                        CreatedDate = address.CreatedDate,
                    },
                    "Create address successfully"
                );
            }

            return OperationResult<AddressDto>.Failure("Role not supported");
        }

        public async Task<OperationResult<AddressDto>> UpdateAsync(
            Guid addressId,
            Guid userId,
            UpdateAddressRequestDto dto,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByIdWithProfilesAsync(userId, cancellationToken);

            if (user == null)
            {
                return OperationResult<AddressDto>.Failure("User not found");
            }

            var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);

            if (address == null || address.IsDeleted)
            {
                return OperationResult<AddressDto>.Failure("Address not found");
            }

            var roles = user.UserRoles.Select(x => x.Role!.Name).ToList();

            var isCustomer = roles.Contains("CUSTOMER");
            var isWorker = roles.Contains("WORKER");

            // =========================
            // Authorization
            // =========================

            var hasPermission = false;

            if (
                isCustomer
                && user.CustomerProfile != null
                && address.CustomerProfileId == user.CustomerProfile.Id
            )
            {
                hasPermission = true;
            }

            if (
                isWorker
                && user.WorkerProfile != null
                && address.WorkerProfileId == user.WorkerProfile.Id
            )
            {
                hasPermission = true;
            }

            if (!hasPermission)
            {
                return OperationResult<AddressDto>.Failure("Access denied");
            }

            // =========================
            // Customer Default Address
            // =========================

            if (isCustomer && dto.IsDefault && !address.IsDefault)
            {
                var currentDefault = await _addressRepository.GetDefaultByCustomerProfileIdAsync(
                    user.CustomerProfile!.Id,
                    cancellationToken
                );

                if (currentDefault != null && currentDefault.Id != address.Id)
                {
                    currentDefault.IsDefault = false;

                    _addressRepository.Update(currentDefault);
                }
            }

            // =========================
            // Update
            // =========================

            address.Label = dto.Label;
            address.City = dto.City;
            address.District = dto.District;
            address.Ward = dto.Ward;
            address.Detail = dto.Detail;
            address.Lat = dto.Lat;
            address.Lng = dto.Lng;

            // Worker always default
            address.IsDefault = isWorker ? true : dto.IsDefault;

            _addressRepository.Update(address);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult<AddressDto>.Success(
                new AddressDto
                {
                    Id = address.Id,
                    Label = address.Label,
                    City = address.City,
                    District = address.District,
                    Ward = address.Ward,
                    Detail = address.Detail,
                    Lat = address.Lat,
                    Lng = address.Lng,
                    IsDefault = address.IsDefault,
                    CreatedDate = address.CreatedDate,
                },
                "Update address successfully"
            );
        }

        public async Task<OperationResult> DeleteAsync(
            Guid addressId,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.GetByIdWithProfilesAsync(userId, cancellationToken);

            if (user == null)
            {
                return OperationResult.Failure("User not found");
            }

            var address = await _addressRepository.GetByIdAsync(addressId, cancellationToken);

            if (address == null || address.IsDeleted)
            {
                return OperationResult.Failure("Address not found");
            }

            var roles = user.UserRoles.Select(x => x.Role!.Name).ToList();

            var isCustomer = roles.Contains("CUSTOMER");
            var isWorker = roles.Contains("WORKER");

            // =========================
            // Authorization
            // =========================

            var hasPermission = false;

            if (
                isCustomer
                && user.CustomerProfile != null
                && address.CustomerProfileId == user.CustomerProfile.Id
            )
            {
                hasPermission = true;
            }

            if (
                isWorker
                && user.WorkerProfile != null
                && address.WorkerProfileId == user.WorkerProfile.Id
            )
            {
                hasPermission = true;
            }

            if (!hasPermission)
            {
                return OperationResult.Failure("Access denied");
            }

            // =========================
            // Worker Restriction
            // =========================

            if (isWorker)
            {
                return OperationResult.Failure("Worker address cannot be deleted");
            }

            // =========================
            // Delete
            // =========================

            address.IsDeleted = true;
            address.DeletedDate = DateTime.UtcNow;

            _addressRepository.Update(address);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return OperationResult.Success("Delete address successfully");
        }
    }
}
