using Application.Common;

namespace Application.DTOs.WorkerProfile
{
    public class CustomerWorkerSearchQuery : PagedQuery
    {
        // Tìm kiếm danh mục & từ khóa
        public Guid? CategoryId { get; set; }

        // Bộ lọc vị trí & bán kính
        public double? CustomerLat { get; set; }
        public double? CustomerLng { get; set; }
        public double? RadiusKm { get; set; }

        // Bộ lọc khu vực hành chính (không dùng tọa độ GPS)
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }

        // Bộ lọc khoảng giá dịch vụ
        public long? MinPrice { get; set; }
        public long? MaxPrice { get; set; }

        // Bộ lọc chất lượng dịch vụ & trạng thái
        public double? MinRating { get; set; }
        public bool? IsOnline { get; set; }
    }
}
