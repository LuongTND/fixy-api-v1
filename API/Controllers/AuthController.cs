using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [AllowAnonymous]
    public class AuthController : ApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.SignupAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("verify-signup-otp")]
        public async Task<IActionResult> VerifySignupOtp([FromBody] VerifySignupOtpDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.VerifySignupOtpAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.LogoutAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("request-password-otp")]
        public async Task<IActionResult> RequestPasswordOtp([FromBody] RequestPasswordOtpDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.RequestPasswordOtpAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        [HttpPost("change-password-by-otp")]
        public async Task<IActionResult> ChangePasswordByOtp([FromBody] ChangePasswordByOtpDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.ChangePasswordByOtpAsync(request, GetIpAddress(), cancellationToken);
            return HandleResult(result);
        }

        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
