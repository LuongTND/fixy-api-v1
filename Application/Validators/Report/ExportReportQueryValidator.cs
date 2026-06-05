using Application.DTOs.Report;
using FluentValidation;

namespace Application.Validators.Report
{
    public class ExportReportQueryValidator : AbstractValidator<ExportReportQuery>
    {
        public ExportReportQueryValidator()
        {
            RuleFor(x => x.Format)
                .IsInEnum()
                .WithMessage("Định dạng xuất không hỗ trợ. Chỉ chấp nhận: Csv, Xlsx, Pdf.");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Ngày bắt đầu không được để trống.");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("Ngày kết thúc không được để trống.")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.")
                .Must(
                    (query, endDate) => (endDate - query.StartDate).TotalDays <= 366
                )
                .WithMessage(
                    "Khoảng thời gian xuất báo cáo không được lớn hơn 1 năm."
                );

            RuleFor(x => x.ReportType)
                .IsInEnum()
                .WithMessage("Loại báo cáo không hợp lệ.");
        }
    }
}
