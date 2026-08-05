using Domain.Entity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/seed")]
    [Authorize(Roles = "ADMIN")]
    public class SpaDataSeederController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpaDataSeederController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("spa-data")]
        public async Task<IActionResult> SeedSpaData(CancellationToken cancellationToken)
        {
            // Check if already seeded
            if (await _context.SpaServiceCategories.AnyAsync(cancellationToken))
            {
                return Ok(new { message = "Spa data already seeded. Skipping." });
            }

            // ─────────────────────────────────
            // 1. Seed SpaServiceCategories
            // ─────────────────────────────────
            var catMassage = new SpaServiceCategory
            {
                Id = Guid.NewGuid(),
                Name = "Massage & Xoa bóp",
                Description = "Massage body, massage chân, massage đầu, giác hơi",
                Code = "massage",
                SortOrder = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var catSkinCare = new SpaServiceCategory
            {
                Id = Guid.NewGuid(),
                Name = "Chăm sóc da mặt",
                Description = "Facial, trị mụn, trẻ hóa da, tẩy tế bào chết",
                Code = "cham-soc-da",
                SortOrder = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var catTherapy = new SpaServiceCategory
            {
                Id = Guid.NewGuid(),
                Name = "Trị liệu & Phục hồi",
                Description = "Vật lý trị liệu, bấm huyệt, đá nóng, thảo dược",
                Code = "tri-lieu",
                SortOrder = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var catNail = new SpaServiceCategory
            {
                Id = Guid.NewGuid(),
                Name = "Nail & Làm móng",
                Description = "Sơn gel, đắp bột, nail art, chăm sóc móng",
                Code = "nail",
                SortOrder = 4,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var catHair = new SpaServiceCategory
            {
                Id = Guid.NewGuid(),
                Name = "Tóc & Tạo kiểu",
                Description = "Gội đầu dưỡng sinh, ủ tóc, uốn duỗi, nhuộm",
                Code = "toc",
                SortOrder = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var catWax = new SpaServiceCategory
            {
                Id = Guid.NewGuid(),
                Name = "Wax & Triệt lông",
                Description = "Wax lông, triệt lông vĩnh viễn, tẩy lông",
                Code = "wax",
                SortOrder = 6,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var categories = new[] { catMassage, catSkinCare, catTherapy, catNail, catHair, catWax };
            await _context.SpaServiceCategories.AddRangeAsync(categories, cancellationToken);

            // ─────────────────────────────────
            // 2. Seed SpaPartners
            // ─────────────────────────────────
            var spa1 = new SpaPartner
            {
                Id = Guid.NewGuid(),
                Name = "Lavender Spa Đà Nẵng",
                Description = "Spa cao cấp tại trung tâm Đà Nẵng, chuyên massage trị liệu và chăm sóc da mặt với sản phẩm organic nhập khẩu từ Pháp.",
                Address = "123 Nguyễn Văn Linh, Hải Châu, Đà Nẵng",
                City = "Đà Nẵng",
                Lat = 16.0544,
                Lng = 108.2022,
                Phone = "0236 1234 567",
                Email = "info@lavenderspa.vn",
                OpeningHours = "09:00 - 21:00",
                RatingAvg = 4.8,
                TotalReviews = 156,
                SortOrder = 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var spa2 = new SpaPartner
            {
                Id = Guid.NewGuid(),
                Name = "Golden Lotus Spa",
                Description = "Không gian yên tĩnh giữa lòng phố biển, đặc biệt các liệu trình massage đá nóng và thảo dược truyền thống.",
                Address = "45 Trần Phú, Hải Châu, Đà Nẵng",
                City = "Đà Nẵng",
                Lat = 16.0678,
                Lng = 108.2208,
                Phone = "0236 9876 543",
                Email = "hello@goldenlotus.vn",
                OpeningHours = "08:00 - 22:00",
                RatingAvg = 4.6,
                TotalReviews = 89,
                SortOrder = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var spa3 = new SpaPartner
            {
                Id = Guid.NewGuid(),
                Name = "Zen Garden Spa & Wellness",
                Description = "Thiên đường spa phong cách Nhật Bản, onsen riêng tư, massage shiatsu và các liệu trình tắm khoáng.",
                Address = "88 Võ Nguyên Giáp, Sơn Trà, Đà Nẵng",
                City = "Đà Nẵng",
                Lat = 16.0651,
                Lng = 108.2468,
                Phone = "0236 5555 888",
                OpeningHours = "10:00 - 22:00",
                RatingAvg = 4.9,
                TotalReviews = 234,
                SortOrder = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var spa4 = new SpaPartner
            {
                Id = Guid.NewGuid(),
                Name = "Beauty Queen Spa",
                Description = "Chuyên chăm sóc da mặt, nail art cao cấp và các dịch vụ làm đẹp toàn diện cho phái nữ.",
                Address = "201 Lê Duẩn, Thanh Khê, Đà Nẵng",
                City = "Đà Nẵng",
                Lat = 16.0610,
                Lng = 108.1918,
                Phone = "0236 7777 999",
                OpeningHours = "09:00 - 20:00",
                RatingAvg = 4.5,
                TotalReviews = 67,
                SortOrder = 4,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var spa5 = new SpaPartner
            {
                Id = Guid.NewGuid(),
                Name = "Herbal Retreat Spa",
                Description = "Spa trị liệu bằng thảo dược thiên nhiên, chuyên các liệu trình phục hồi sức khỏe và giảm stress.",
                Address = "56 Bạch Đằng, Hải Châu, Đà Nẵng",
                City = "Đà Nẵng",
                Lat = 16.0720,
                Lng = 108.2230,
                Phone = "0236 3333 222",
                OpeningHours = "08:30 - 21:30",
                RatingAvg = 4.7,
                TotalReviews = 112,
                SortOrder = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var spas = new[] { spa1, spa2, spa3, spa4, spa5 };
            await _context.SpaPartners.AddRangeAsync(spas, cancellationToken);

            // ─────────────────────────────────
            // 3. Seed SpaPartnerServices
            // ─────────────────────────────────
            var services = new List<SpaPartnerService>
            {
                // Spa 1 - Lavender
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa1.Id, SpaServiceCategoryId = catMassage.Id, Name = "Massage body thư giãn", Description = "60 phút massage toàn thân với tinh dầu lavender", Price = 350000, DiscountedPrice = 299000, DurationMinutes = 60, SortOrder = 1, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa1.Id, SpaServiceCategoryId = catMassage.Id, Name = "Massage chân đá nóng", Description = "45 phút massage chân kết hợp đá nóng", Price = 250000, DurationMinutes = 45, SortOrder = 2, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa1.Id, SpaServiceCategoryId = catSkinCare.Id, Name = "Facial dưỡng ẩm chuyên sâu", Description = "Làm sạch, tẩy tế bào chết, đắp mặt nạ collagen", Price = 450000, DiscountedPrice = 380000, DurationMinutes = 75, SortOrder = 3, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa1.Id, SpaServiceCategoryId = catWax.Id, Name = "Wax toàn thân", Description = "Wax lông toàn thân bằng sáp mật ong organic", Price = 500000, DurationMinutes = 90, SortOrder = 4, IsActive = true },

                // Spa 2 - Golden Lotus
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa2.Id, SpaServiceCategoryId = catMassage.Id, Name = "Massage đá nóng truyền thống", Description = "75 phút massage đá bazan nóng", Price = 400000, DiscountedPrice = 350000, DurationMinutes = 75, SortOrder = 1, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa2.Id, SpaServiceCategoryId = catTherapy.Id, Name = "Trị liệu bấm huyệt", Description = "60 phút bấm huyệt châm cứu theo y học cổ truyền", Price = 380000, DurationMinutes = 60, SortOrder = 2, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa2.Id, SpaServiceCategoryId = catSkinCare.Id, Name = "Chăm sóc da mặt cơ bản", Description = "Rửa mặt, hút mụn, đắp mặt nạ", Price = 280000, DurationMinutes = 50, SortOrder = 3, IsActive = true },

                // Spa 3 - Zen Garden
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa3.Id, SpaServiceCategoryId = catMassage.Id, Name = "Massage Shiatsu Nhật Bản", Description = "90 phút massage shiatsu chính thống", Price = 550000, DiscountedPrice = 480000, DurationMinutes = 90, SortOrder = 1, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa3.Id, SpaServiceCategoryId = catTherapy.Id, Name = "Tắm khoáng onsen", Description = "60 phút tắm khoáng nóng private onsen", Price = 600000, DurationMinutes = 60, SortOrder = 2, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa3.Id, SpaServiceCategoryId = catTherapy.Id, Name = "Liệu trình thảo dược Nhật", Description = "120 phút trị liệu toàn diện với thảo dược", Price = 800000, DiscountedPrice = 680000, DurationMinutes = 120, SortOrder = 3, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa3.Id, SpaServiceCategoryId = catHair.Id, Name = "Gội đầu dưỡng sinh", Description = "45 phút gội đầu thảo dược kết hợp massage đầu", Price = 200000, DurationMinutes = 45, SortOrder = 4, IsActive = true },

                // Spa 4 - Beauty Queen
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa4.Id, SpaServiceCategoryId = catSkinCare.Id, Name = "Trẻ hóa da Collagen", Description = "90 phút liệu trình trẻ hóa da bằng collagen tươi", Price = 650000, DiscountedPrice = 550000, DurationMinutes = 90, SortOrder = 1, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa4.Id, SpaServiceCategoryId = catNail.Id, Name = "Nail art cao cấp", Description = "Sơn gel, vẽ nail art theo yêu cầu", Price = 350000, DurationMinutes = 60, SortOrder = 2, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa4.Id, SpaServiceCategoryId = catNail.Id, Name = "Chăm sóc móng cơ bản", Description = "Cắt da, dũa móng, sơn gel đơn sắc", Price = 180000, DurationMinutes = 40, SortOrder = 3, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa4.Id, SpaServiceCategoryId = catWax.Id, Name = "Triệt lông vĩnh viễn", Description = "Triệt lông bằng công nghệ IPL an toàn", Price = 800000, DurationMinutes = 45, SortOrder = 4, IsActive = true },

                // Spa 5 - Herbal Retreat
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa5.Id, SpaServiceCategoryId = catTherapy.Id, Name = "Xông hơi thảo dược", Description = "30 phút xông hơi với 12 loại thảo dược Việt Nam", Price = 200000, DurationMinutes = 30, SortOrder = 1, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa5.Id, SpaServiceCategoryId = catMassage.Id, Name = "Massage thảo dược", Description = "90 phút massage body với dầu thảo dược tự nhiên", Price = 450000, DiscountedPrice = 380000, DurationMinutes = 90, SortOrder = 2, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa5.Id, SpaServiceCategoryId = catTherapy.Id, Name = "Liệu trình giảm stress", Description = "120 phút kết hợp massage, xông hơi, thiền", Price = 700000, DiscountedPrice = 600000, DurationMinutes = 120, SortOrder = 3, IsActive = true },
                new() { Id = Guid.NewGuid(), SpaPartnerId = spa5.Id, SpaServiceCategoryId = catHair.Id, Name = "Ủ tóc phục hồi", Description = "60 phút ủ tóc với dầu argan và keratin", Price = 300000, DurationMinutes = 60, SortOrder = 4, IsActive = true },
            };

            await _context.SpaPartnerServices.AddRangeAsync(services, cancellationToken);

            // ─────────────────────────────────
            // 4. Seed SpaPartnerPromotions
            // ─────────────────────────────────
            var promotions = new List<SpaPartnerPromotion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    SpaPartnerId = spa1.Id,
                    Title = "Ưu đãi giờ thấp điểm",
                    Description = "Giảm 20% tất cả dịch vụ massage vào buổi sáng",
                    DiscountPercent = 20,
                    OffPeakStartTime = new TimeOnly(8, 0),
                    OffPeakEndTime = new TimeOnly(11, 0),
                    StartsAt = DateTime.UtcNow.AddDays(-7),
                    ExpiresAt = DateTime.UtcNow.AddDays(90),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SpaPartnerId = spa3.Id,
                    Title = "Khuyến mãi mùa hè",
                    Description = "Giảm 15% tất cả liệu trình trị liệu",
                    DiscountPercent = 15,
                    StartsAt = DateTime.UtcNow.AddDays(-3),
                    ExpiresAt = DateTime.UtcNow.AddDays(60),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    SpaPartnerId = spa5.Id,
                    Title = "Flash Sale cuối tuần",
                    Description = "Giảm 30% combo massage + xông hơi thảo dược",
                    DiscountPercent = 30,
                    OffPeakStartTime = new TimeOnly(14, 0),
                    OffPeakEndTime = new TimeOnly(17, 0),
                    StartsAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },
            };

            await _context.SpaPartnerPromotions.AddRangeAsync(promotions, cancellationToken);

            // ─────────────────────────────────
            // Save all
            // ─────────────────────────────────
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Spa data seeded successfully",
                data = new
                {
                    categoriesCount = categories.Length,
                    spasCount = spas.Length,
                    servicesCount = services.Count,
                    promotionsCount = promotions.Count
                }
            });
        }
    }
}
