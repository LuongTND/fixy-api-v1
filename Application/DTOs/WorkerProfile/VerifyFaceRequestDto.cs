using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.WorkerProfile;

public class VerifyFaceRequestDto
{
    [Required(ErrorMessage = "Ảnh mặt trước CCCD là bắt buộc")]
    public IFormFile CardFrontImage { get; set; } = null!;

    [Required(ErrorMessage = "Ảnh chân dung (Selfie) là bắt buộc")]
    public IFormFile SelfieImage { get; set; } = null!;
}
