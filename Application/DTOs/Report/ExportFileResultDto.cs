namespace Application.DTOs.Report
{
    public class ExportFileResultDto
    {
        public byte[] FileContents { get; set; } = Array.Empty<byte>();

        public string ContentType { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;
    }
}
