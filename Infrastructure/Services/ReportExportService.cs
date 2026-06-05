using System.Globalization;
using System.Text;
using Application.Common;
using Application.DTOs.Report;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Enum;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class ReportExportService : IReportExportService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public ReportExportService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<OperationResult<ExportFileResultDto>> ExportBookingsReportAsync(
            ExportFormat format,
            DateTime startDate,
            DateTime endDate
        )
        {
            var data = await _dashboardRepository.GetBookingsForReportAsync(startDate, endDate);

            if (data.Count == 0)
            {
                return OperationResult<ExportFileResultDto>.Failure(
                    "Không có dữ liệu trong khoảng thời gian đã chọn."
                );
            }

            var timestamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

            byte[] fileBytes;
            string contentType;
            string fileName;

            switch (format)
            {
                case ExportFormat.Csv:
                    fileBytes = GenerateCsv(data);
                    contentType = "text/csv";
                    fileName = $"BaoCao_DonHang_{timestamp}.csv";
                    break;

                case ExportFormat.Xlsx:
                    fileBytes = GenerateExcel(data, startDate, endDate);
                    contentType =
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"BaoCao_DonHang_{timestamp}.xlsx";
                    break;

                case ExportFormat.Pdf:
                    fileBytes = GeneratePdf(data, startDate, endDate);
                    contentType = "application/pdf";
                    fileName = $"BaoCao_DonHang_{timestamp}.pdf";
                    break;

                default:
                    return OperationResult<ExportFileResultDto>.Failure(
                        "Định dạng xuất không hỗ trợ."
                    );
            }

            var result = new ExportFileResultDto
            {
                FileContents = fileBytes,
                ContentType = contentType,
                FileName = fileName,
            };

            return OperationResult<ExportFileResultDto>.Success(result);
        }

        // =====================================================
        // CSV Export Engine
        // =====================================================

        private static byte[] GenerateCsv(List<BookingReportDto> data)
        {
            using var memoryStream = new MemoryStream();

            // Write UTF-8 BOM so Excel auto-detects Vietnamese characters
            var bom = Encoding.UTF8.GetPreamble();
            memoryStream.Write(bom, 0, bom.Length);

            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using var csv = new CsvWriter(writer, config);

                // Write Vietnamese header row manually
                csv.WriteField("Mã Đơn");
                csv.WriteField("Khách Hàng");
                csv.WriteField("Thợ Sửa");
                csv.WriteField("Dịch Vụ");
                csv.WriteField("Địa Chỉ");
                csv.WriteField("Trạng Thái");
                csv.WriteField("Giá Ước Tính");
                csv.WriteField("Giá Cuối Cùng");
                csv.WriteField("Ngày Tạo");
                csv.WriteField("Ngày Hoàn Thành");
                csv.NextRecord();

                foreach (var item in data)
                {
                    csv.WriteField(item.BookingId.ToString()[..8]);
                    csv.WriteField(item.CustomerName);
                    csv.WriteField(item.WorkerName);
                    csv.WriteField(item.CategoryName);
                    csv.WriteField(item.Address);
                    csv.WriteField(item.Status);
                    csv.WriteField(item.EstimatedPrice);
                    csv.WriteField(item.FinalPrice);
                    csv.WriteField(item.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
                    csv.WriteField(
                        item.CompletedAt?.ToString("dd/MM/yyyy HH:mm") ?? ""
                    );
                    csv.NextRecord();
                }

                writer.Flush();
            }

            return memoryStream.ToArray();
        }

        // =====================================================
        // Excel Export Engine
        // =====================================================

        private static byte[] GenerateExcel(
            List<BookingReportDto> data,
            DateTime startDate,
            DateTime endDate
        )
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo Cáo Đơn Hàng");

            // === Title Banner ===
            worksheet.Cell("A1").Value = "BÁO CÁO CHI TIẾT ĐƠN HÀNG - HỆ THỐNG FIXY";
            worksheet
                .Range("A1:J1")
                .Merge()
                .Style.Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Alignment.SetVertical(XLAlignmentVerticalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FF8228"))
                .Font.SetFontColor(XLColor.White);
            worksheet.Row(1).Height = 40;

            // === Subtitle with date range ===
            worksheet.Cell("A2").Value =
                $"Từ ngày {startDate:dd/MM/yyyy} đến ngày {endDate:dd/MM/yyyy}  •  Tổng: {data.Count} đơn  •  Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet
                .Range("A2:J2")
                .Merge()
                .Style.Font.SetItalic()
                .Font.SetFontSize(10)
                .Font.SetFontColor(XLColor.FromHtml("#555555"))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            worksheet.Row(2).Height = 22;

            // === Headers ===
            var headers = new[]
            {
                "STT",
                "Mã Đơn",
                "Khách Hàng",
                "Thợ Sửa",
                "Dịch Vụ",
                "Địa Chỉ",
                "Trạng Thái",
                "Giá Ước Tính",
                "Giá Cuối Cùng",
                "Ngày Tạo",
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(4, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FF8228");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E6741F");
            }
            worksheet.Row(4).Height = 28;

            // === Data Rows ===
            int currentRow = 5;
            int index = 1;
            foreach (var item in data)
            {
                var bgColor =
                    index % 2 == 0
                        ? XLColor.White
                        : XLColor.FromHtml("#FFF5EC");

                worksheet.Cell(currentRow, 1).Value = index;
                worksheet.Cell(currentRow, 2).Value = item.BookingId.ToString()[..8];
                worksheet.Cell(currentRow, 3).Value = item.CustomerName;
                worksheet.Cell(currentRow, 4).Value = item.WorkerName;
                worksheet.Cell(currentRow, 5).Value = item.CategoryName;
                worksheet.Cell(currentRow, 6).Value = item.Address;
                worksheet.Cell(currentRow, 7).Value = item.Status;

                var cellEst = worksheet.Cell(currentRow, 8);
                cellEst.Value = item.EstimatedPrice;
                cellEst.Style.NumberFormat.Format = "#,##0 \"đ\"";
                cellEst.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                var cellFinal = worksheet.Cell(currentRow, 9);
                cellFinal.Value = item.FinalPrice;
                cellFinal.Style.NumberFormat.Format = "#,##0 \"đ\"";
                cellFinal.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                worksheet.Cell(currentRow, 10).Value = item.CreatedAt;
                worksheet.Cell(currentRow, 10).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                // Apply alternating row color and border
                var rowRange = worksheet.Range(currentRow, 1, currentRow, 10);
                rowRange.Style.Fill.BackgroundColor = bgColor;
                rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#D6DCE4");
                rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                currentRow++;
                index++;
            }

            // === Summary Row ===
            var summaryRow = currentRow + 1;
            worksheet.Cell(summaryRow, 7).Value = "TỔNG CỘNG:";
            worksheet.Cell(summaryRow, 7).Style.Font.Bold = true;
            worksheet.Cell(summaryRow, 7).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Right;

            var totalCell = worksheet.Cell(summaryRow, 9);
            totalCell.FormulaA1 = $"SUM(I5:I{currentRow - 1})";
            totalCell.Style.NumberFormat.Format = "#,##0 \"đ\"";
            totalCell.Style.Font.Bold = true;
            totalCell.Style.Font.FontColor = XLColor.FromHtml("#FF8228");
            totalCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // === Auto-fit columns ===
            worksheet.Columns().AdjustToContents();

            // Cap max width to prevent extremely wide columns
            foreach (var col in worksheet.Columns())
            {
                if (col.Width > 40)
                    col.Width = 40;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // =====================================================
        // PDF Export Engine
        // =====================================================

        private static byte[] GeneratePdf(
            List<BookingReportDto> data,
            DateTime startDate,
            DateTime endDate
        )
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4.Landscape());

                    // --- Header ---
                    page.Header()
                        .PaddingBottom(10)
                        .Column(col =>
                        {
                            col.Item()
                                .Text("BÁO CÁO CHI TIẾT ĐƠN HÀNG")
                                .Bold()
                                .FontSize(18)
                                .FontColor(Color.FromHex("#FF8228"));

                            col.Item()
                                .Text("HỆ THỐNG KẾT NỐI THỢ FIXY")
                                .FontSize(11)
                                .FontColor(Colors.Grey.Darken1);

                            col.Item()
                                .PaddingTop(4)
                                .Text(
                                    $"Từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}  •  Tổng: {data.Count} đơn  •  Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}"
                                )
                                .FontSize(9)
                                .Italic()
                                .FontColor(Colors.Grey.Medium);

                            col.Item()
                                .PaddingTop(6)
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);
                        });

                    // --- Content ---
                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30); // STT
                            columns.ConstantColumn(55); // Mã Đơn
                            columns.RelativeColumn(2); // Khách hàng
                            columns.RelativeColumn(2); // Thợ sửa
                            columns.RelativeColumn(1.8f); // Dịch vụ
                            columns.RelativeColumn(3); // Địa chỉ
                            columns.RelativeColumn(1.2f); // Trạng thái
                            columns.RelativeColumn(1.3f); // Giá cuối
                            columns.RelativeColumn(1.5f); // Ngày tạo
                        });

                        // Table Header
                        table.Header(header =>
                        {
                            var headerStyle = TextStyle
                                .Default.FontSize(8)
                                .Bold()
                                .FontColor(Colors.White);

                            void HeaderCell(
                                IContainer container,
                                string text
                            )
                            {
                                container
                                    .Background(Color.FromHex("#FF8228"))
                                    .Padding(5)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text(text)
                                    .Style(headerStyle);
                            }

                            HeaderCell(header.Cell(), "STT");
                            HeaderCell(header.Cell(), "Mã Đơn");
                            HeaderCell(header.Cell(), "Khách Hàng");
                            HeaderCell(header.Cell(), "Thợ Sửa");
                            HeaderCell(header.Cell(), "Dịch Vụ");
                            HeaderCell(header.Cell(), "Địa Chỉ");
                            HeaderCell(header.Cell(), "Trạng Thái");
                            HeaderCell(header.Cell(), "Thành Tiền");
                            HeaderCell(header.Cell(), "Ngày Tạo");
                        });

                        // Table Data Rows
                        int rowIndex = 1;
                        foreach (var item in data)
                        {
                            var bgColor =
                                rowIndex % 2 == 0
                                    ? Colors.White
                                    : Colors.Grey.Lighten4;

                            var cellStyle = TextStyle.Default.FontSize(8);

                            void DataCell(
                                IContainer container,
                                string text,
                                bool alignRight = false
                            )
                            {
                                var cell = container
                                    .Background(bgColor)
                                    .BorderBottom(0.5f)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(4);

                                if (alignRight)
                                    cell.AlignRight()
                                        .Text(text)
                                        .Style(cellStyle);
                                else
                                    cell.Text(text).Style(cellStyle);
                            }

                            DataCell(table.Cell(), rowIndex.ToString());
                            DataCell(
                                table.Cell(),
                                item.BookingId.ToString()[..8]
                            );
                            DataCell(table.Cell(), item.CustomerName);
                            DataCell(table.Cell(), item.WorkerName);
                            DataCell(table.Cell(), item.CategoryName);
                            DataCell(table.Cell(), item.Address);
                            DataCell(table.Cell(), item.Status);
                            DataCell(
                                table.Cell(),
                                $"{item.FinalPrice:N0} đ",
                                alignRight: true
                            );
                            DataCell(
                                table.Cell(),
                                item.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                            );

                            rowIndex++;
                        }
                    });

                    // --- Footer ---
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" / ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
