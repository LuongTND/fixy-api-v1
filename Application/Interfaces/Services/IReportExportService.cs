using Application.Common;
using Application.DTOs.Report;
using Domain.Enum;

namespace Application.Interfaces.Services
{
    public interface IReportExportService
    {
        Task<OperationResult<ExportFileResultDto>> ExportBookingsReportAsync(
            ExportFormat format,
            DateTime startDate,
            DateTime endDate
        );
    }
}
