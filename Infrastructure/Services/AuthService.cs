using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services.Auth;
using Application.Settings;
using Domain.Entity;
using Domain.Enum;
using Google.Apis.Auth;
using Infrastructure.Helpers;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly GoogleSettings _googleSettings;
        private readonly FacebookSettings _facebookSettings;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(
            IUserRepository userRepository,
            ICustomerProfileRepository customerProfileRepository,
            IUserOtpRepository userOtpRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IOptions<GoogleSettings> googleSettings,
            IOptions<FacebookSettings> facebookSettings,
            IOptions<JwtSettings> jwtSettings,
            IHttpClientFactory httpClientFactory
        )
        {
            _userRepository = userRepository;
            _customerProfileRepository = customerProfileRepository;
            _userOtpRepository = userOtpRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _googleSettings = googleSettings.Value;
            _facebookSettings = facebookSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<OperationResult<AuthResponseDto>> RegisterAsync(
            RegisterRequestDto request,
            CancellationToken ct
        )
        {
            var otp = await _userOtpRepository.GetVerifiedOtpAsync(request.Target, ct);

            if (otp == null)
                return OperationResult<AuthResponseDto>.Failure("OTP is not verified");

            var existedUser = await _userRepository.GetByTargetAsync(request.Target, ct);

            if (existedUser != null)
                return OperationResult<AuthResponseDto>.Failure("Account already exists");

            var user = new User
            {
                FullName = request.FullName,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
            };

            if (IsPhone(request.Target))
            {
                user.Phone = request.Target;
                user.IsPhoneVerified = true;
            }
            else if (IsEmail(request.Target))
            {
                user.Email = request.Target;
                user.IsEmailVerified = true;
            }
            else
            {
                return OperationResult<AuthResponseDto>.Failure("Invalid email or phone number");
            }
            await _userRepository.AddAsync(user, ct);

            var role =
                request.RoleRegister == RoleRegister.Worker
                    ? await _roleRepository.GetWorkerRoleAsync(ct)
                    : await _roleRepository.GetCustomerRoleAsync(ct);

            await _userRoleRepository.AddAsync(new UserRole { User = user, RoleId = role.Id }, ct);

            await _walletRepository.AddAsync(
                new Wallet
                {
                    UserId = user.Id,
                    OwnerType =
                        request.RoleRegister == RoleRegister.Customer
                            ? WalletOwnerType.Customer
                            : WalletOwnerType.Worker,
                    Balance = 0,
                    LifetimeEarned = 0,
                    LifetimeSpent = 0,
                    CreatedAt = DateTime.UtcNow,
                },
                ct
            );
            if (request.RoleRegister == RoleRegister.Customer)
            {
                await _customerProfileRepository.AddAsync(
                    new CustomerProfile { UserId = user.Id },
                    ct
                );
            }
            var accessToken = _jwtService.GenerateAccessToken(user, new[] { role.Name });
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    User = user,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = new List<string> { role.Name },
                },
                "Register successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> LoginAsync(
            LoginRequestDto request,
            CancellationToken ct
        )
        {
            var user = await _userRepository.GetByTargetWithRoleAsync(request.Target, ct);

            if (user == null)
                return OperationResult<AuthResponseDto>.Failure("Invalid credentials");

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return OperationResult<AuthResponseDto>.Failure("Invalid credentials");
            if (!user.IsActive)
                return OperationResult<AuthResponseDto>.Failure("Your account is inactive");

            var roles = user.UserRoles.Select(x => x.Role!.Name).ToList();

            var accessToken = _jwtService.GenerateAccessToken(user, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = roles,
                },
                "Login successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> GoogleLoginAsync(
            GoogleLoginRequestDto request,
            CancellationToken ct
        )
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                request.Credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.WebClientId, _googleSettings.IosClientId },
                }
            );

            var user = await _userRepository.GetByTargetAsync(payload.Email, ct);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    IsEmailVerified = true,
                };

                await _userRepository.AddAsync(user, ct);
            }

            var role = await _roleRepository.GetCustomerRoleAsync(ct);

            var exists = await _userRoleRepository.ExistsAsync(
                ur => ur.UserId == user.Id && ur.RoleId == role.Id,
                ct
            );

            if (!exists)
            {
                await _userRoleRepository.AddAsync(
                    new UserRole { UserId = user.Id, RoleId = role.Id },
                    ct
                );
            }
            if (!user.IsActive)
                return OperationResult<AuthResponseDto>.Failure("Your account is inactive");
            var accessToken = _jwtService.GenerateAccessToken(user, new[] { role.Name });
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = new List<string> { role.Name },
                },
                "Login successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> FacebookLoginAsync(
            FacebookLoginRequestDto request,
            CancellationToken ct
        )
        {
            // Step 1: Validate token and get user info from Facebook Graph API
            var httpClient = _httpClientFactory.CreateClient();
            var graphUrl = $"https://graph.facebook.com/v19.0/me?fields=id,name,email,picture.type(large)&access_token={request.AccessToken}";

            var response = await httpClient.GetAsync(graphUrl, ct);

            if (!response.IsSuccessStatusCode)
                return OperationResult<AuthResponseDto>.Failure("Invalid Facebook access token");

            var json = await response.Content.ReadAsStringAsync(ct);
            var fbUser = JsonSerializer.Deserialize<JsonElement>(json);

            var facebookId = fbUser.GetProperty("id").GetString();
            var name = fbUser.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "";
            var email = fbUser.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
            var avatarUrl = fbUser.TryGetProperty("picture", out var picProp)
                && picProp.TryGetProperty("data", out var dataProp)
                && dataProp.TryGetProperty("url", out var urlProp)
                ? urlProp.GetString()
                : null;

            if (string.IsNullOrEmpty(facebookId))
                return OperationResult<AuthResponseDto>.Failure("Could not retrieve Facebook user ID");

            // Step 2: Find existing user by OAuthId
            var user = await _userRepository.GetByOAuthIdAsync(OAuthProvider.Facebook, facebookId, ct);

            if (user == null && !string.IsNullOrEmpty(email))
            {
                // Check if a user with this email already exists (link accounts)
                user = await _userRepository.GetByTargetAsync(email, ct);
                if (user != null)
                {
                    user.OAuthProvider = OAuthProvider.Facebook;
                    user.OAuthId = facebookId;
                    if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(avatarUrl))
                        user.AvatarUrl = avatarUrl;
                }
            }

            if (user == null)
            {
                // Step 3: Create new user
                user = new User
                {
                    Email = email,
                    FullName = name ?? "",
                    IsEmailVerified = !string.IsNullOrEmpty(email),
                    OAuthProvider = OAuthProvider.Facebook,
                    OAuthId = facebookId,
                    AvatarUrl = avatarUrl,
                };

                await _userRepository.AddAsync(user, ct);

                // Create wallet for new user
                await _walletRepository.AddAsync(
                    new Wallet
                    {
                        UserId = user.Id,
                        OwnerType = WalletOwnerType.Customer,
                        Balance = 0,
                        LifetimeEarned = 0,
                        LifetimeSpent = 0,
                        CreatedAt = DateTime.UtcNow,
                    },
                    ct
                );

                // Create customer profile
                await _customerProfileRepository.AddAsync(
                    new CustomerProfile { UserId = user.Id },
                    ct
                );
            }

            if (!user.IsActive)
                return OperationResult<AuthResponseDto>.Failure("Your account is inactive");

            // Step 4: Ensure customer role
            var role = await _roleRepository.GetCustomerRoleAsync(ct);

            var exists = await _userRoleRepository.ExistsAsync(
                ur => ur.UserId == user.Id && ur.RoleId == role.Id,
                ct
            );

            if (!exists)
            {
                await _userRoleRepository.AddAsync(
                    new UserRole { UserId = user.Id, RoleId = role.Id },
                    ct
                );
            }

            // Step 5: Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user, new[] { role.Name });
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(refreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = new List<string> { role.Name },
                },
                "Login successfully"
            );
        }

        public async Task<OperationResult<AuthResponseDto>> RefreshTokenAsync(
            string refreshToken,
            CancellationToken ct
        )
        {
            var tokenHash = TokenHasher.Hash(refreshToken);

            var token = await _refreshTokenRepository.GetValidTokenWithUserAsync(tokenHash, ct);

            if (token == null)
                return OperationResult<AuthResponseDto>.Failure("Invalid refresh token");

            if (token.ExpiresAt < DateTime.UtcNow)
                return OperationResult<AuthResponseDto>.Failure("Refresh token expired");

            var user = token.User;

            var roles = user!.UserRoles.Select(x => x.Role!.Name).ToList();

            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            await _refreshTokenRepository.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    TokenHash = TokenHasher.Hash(newRefreshToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                },
                ct
            );

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult<AuthResponseDto>.Success(
                new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Email = user.Email ?? "",
                    UserId = user.Id.ToString(),
                    Roles = roles,
                },
                "Refresh successfully"
            );
        }

        public async Task<OperationResult> ChangePasswordAsync(
            ChangePasswordRequestDto request,
            CancellationToken ct
        )
        {
            var user = await _userRepository.GetByTargetAsync(request.Target, ct);

            if (user == null)
                return OperationResult.Failure("User not found");

            if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
                return OperationResult.Failure("Old password is not correct");

            if (_passwordHasher.VerifyPassword(request.NewPassword, user.PasswordHash))
                return OperationResult.Failure("Match with old password");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult.Success("Change Password Success");
        }

        public async Task<OperationResult> ResetPasswordAsync(
            ResetPasswordRequestDto request,
            CancellationToken ct
        )
        {
            var otp = await _userOtpRepository.GetVerifiedOtpAsync(request.Target, ct);

            if (otp == null)
                return OperationResult.Failure("OTP is not verified");

            var user = await _userRepository.GetByTargetAsync(request.Target, ct);

            if (user == null)
                return OperationResult.Failure("User not found");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            await _unitOfWork.SaveChangesAsync(ct);

            return OperationResult.Success("Reset Password Success");
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
