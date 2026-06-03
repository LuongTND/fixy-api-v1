using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Booking
{
    public class CancelBookingRequest
    {
        [Required(ErrorMessage = "Lý do hủy đơn không được để trống")]
        [MaxLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }
}
