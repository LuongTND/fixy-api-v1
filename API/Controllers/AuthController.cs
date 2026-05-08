using Application.DTOs.Auth;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;

    private readonly IAuthService _authService;

    public AuthController(IOtpService otpService, IAuthService authService)
    {
        _otpService = otpService;
        _authService = authService;
    }

    // =========================
    // OTP
    // =========================

    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto request)
    {
        await _otpService.SendOtpAsync(request.Target, request.Purpose);

        return Ok(new { message = "OTP sent successfully" });
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        await _otpService.VerifyOtpAsync(request.Target, request.OtpCode);

        return Ok(new { message = "OTP verified successfully" });
    }

    // =========================
    // REGISTER
    // =========================

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        return Ok(result);
    }

    // =========================
    // LOGIN
    // =========================

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        return Ok(result);
    }

    // =========================
    // REFRESH TOKEN
    // =========================

    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken
    )
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        return Ok(result);
    }

    // =========================
    // CHANGE PASSWORD
    // =========================

    [HttpPost("password/change")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken
    )
    {
        await _authService.ChangePasswordAsync(request, cancellationToken);

        return Ok(new { message = "Password changed successfully" });
    }

    // =========================
    // RESET PASSWORD
    // =========================
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken
    )
    {
        await _authService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }
}
