namespace Application.DTOs.WorkerProfile;

public class FaceMatchResultDto
{
    public bool IsMatch { get; set; }
    public double Similarity { get; set; } // Thang điểm 0 - 100%
    public bool IsBothFaceFound { get; set; }
    public string? Message { get; set; }
}
