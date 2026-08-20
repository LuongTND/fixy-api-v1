using Application.DTOs.WorkerProfile;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Infrastructure.Services.FaceRecognition;

public class OnnxFaceRecognitionService : IFaceRecognitionService, IDisposable
{
    private readonly ILogger<OnnxFaceRecognitionService> _logger;
    private readonly InferenceSession? _detectorSession;
    private readonly InferenceSession? _recognizerSession;
    private readonly List<Anchor> _priors = new();
    private bool _disposed;

    // UltraFace RFB-320 parameters
    private const int DetectorInputWidth = 320;
    private const int DetectorInputHeight = 240;
    private const float DetectorConfidenceThreshold = 0.70f; // Ngưỡng bắt nhạy chuẩn
    private const float IouThreshold = 0.35f;

    // ArcFace / MobileFaceNet parameters
    private const int RecognizerInputSize = 112;
    private const float MatchCosineThreshold = 0.22f; // Ngưỡng Cosine thực tế cho ảnh CCCD vs Selfie
    private const double MatchPercentThreshold = 70.0; // Thang điểm >= 70% là đạt

    public OnnxFaceRecognitionService(ILogger<OnnxFaceRecognitionService> logger)
    {
        _logger = logger;

        try
        {
            var detectorPath = ResolveModelPath("version-RFB-320.onnx");
            var recognizerPath = ResolveModelPath("arcface_mobilefacenet.onnx");

            var sessionOptions = new SessionOptions
            {
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR
            };

            if (File.Exists(detectorPath))
            {
                _detectorSession = new InferenceSession(detectorPath, sessionOptions);
                GeneratePriors();
                _logger.LogInformation("ONNX UltraFace detector loaded from {Path} ({Size} bytes)", detectorPath, new FileInfo(detectorPath).Length);
                foreach (var inp in _detectorSession.InputMetadata)
                    _logger.LogInformation("  Detector Input: {Name} => [{Dims}]", inp.Key, string.Join(",", inp.Value.Dimensions));
                foreach (var outp in _detectorSession.OutputMetadata)
                    _logger.LogInformation("  Detector Output: {Name} => [{Dims}]", outp.Key, string.Join(",", outp.Value.Dimensions));
            }
            else
            {
                _logger.LogWarning("ONNX Face detector model not found at {Path}", detectorPath);
            }

            if (File.Exists(recognizerPath))
            {
                _recognizerSession = new InferenceSession(recognizerPath, sessionOptions);
                _logger.LogInformation("ONNX ArcFace recognizer loaded from {Path} ({Size} bytes)", recognizerPath, new FileInfo(recognizerPath).Length);
                foreach (var inp in _recognizerSession.InputMetadata)
                    _logger.LogInformation("  Recognizer Input: {Name} => [{Dims}], Type={Type}", inp.Key, string.Join(",", inp.Value.Dimensions), inp.Value.ElementDataType);
                foreach (var outp in _recognizerSession.OutputMetadata)
                    _logger.LogInformation("  Recognizer Output: {Name} => [{Dims}]", outp.Key, string.Join(",", outp.Value.Dimensions));
            }
            else
            {
                _logger.LogWarning("ONNX Face recognizer model not found at {Path}", recognizerPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ONNX Face Recognition models");
        }
    }

    public async Task<FaceMatchResultDto> CompareFacesAsync(
        Stream cardFrontStream,
        Stream selfieStream,
        CancellationToken cancellationToken = default)
    {
        if (_detectorSession == null || _recognizerSession == null)
        {
            return new FaceMatchResultDto
            {
                IsMatch = false,
                Similarity = 0,
                IsBothFaceFound = false,
                Message = "Hệ thống nhận diện khuôn mặt chưa sẵn sàng (Mô hình AI chưa được nạp)."
            };
        }

        try
        {
            cardFrontStream.Position = 0;
            selfieStream.Position = 0;

            using var cardImage = await Image.LoadAsync<Rgb24>(cardFrontStream, cancellationToken);
            using var selfieImage = await Image.LoadAsync<Rgb24>(selfieStream, cancellationToken);

            // 1. Tự động xoay ảnh theo EXIF Orientation
            cardImage.Mutate(x => x.AutoOrient());
            selfieImage.Mutate(x => x.AutoOrient());

            _logger.LogInformation("[DEBUG] Card image: {W}x{H}, Selfie image: {SW}x{SH}",
                cardImage.Width, cardImage.Height, selfieImage.Width, selfieImage.Height);

            // 2. Phát hiện khuôn mặt trên ảnh CCCD (Dùng thuật toán Letterbox bảo toàn tỷ lệ)
            var cardDetection = DetectFaceDetailed(cardImage);
            
            // Fallback nếu ảnh CCCD quá lớn: Quét nửa bên trái CCCD (nơi đặt ảnh chân dung)
            if (cardDetection.BestCandidate == null && cardImage.Width > cardImage.Height)
            {
                int cropW = (int)(cardImage.Width * 0.50);
                var leftRect = new Rectangle(0, 0, cropW, cardImage.Height);
                using var leftHalf = cardImage.Clone(ctx => ctx.Crop(leftRect));
                var leftDetection = DetectFaceDetailed(leftHalf);
                if (leftDetection.BestCandidate != null)
                {
                    var b = leftDetection.BestCandidate.Box;
                    cardDetection = new DetectionResult(
                        new FaceCandidate { Box = new Rectangle(b.X, b.Y, b.Width, b.Height), Score = leftDetection.BestCandidate.Score },
                        1,
                        leftDetection.AreaRatio,
                        false
                    );
                }
            }

            if (cardDetection.BestCandidate == null)
            {
                _logger.LogWarning("[DEBUG] No face found on CCCD card image");
                return new FaceMatchResultDto
                {
                    IsMatch = false,
                    Similarity = 0,
                    IsBothFaceFound = false,
                    Message = "Không tìm thấy khuôn mặt rõ ràng trên ảnh CCCD. Vui lòng kiểm tra lại ảnh chụp."
                };
            }

            _logger.LogInformation("[DEBUG] Card face detected: Box={X},{Y},{W},{H} Score={S:F3} AreaRatio={A:F4}",
                cardDetection.BestCandidate.Box.X, cardDetection.BestCandidate.Box.Y,
                cardDetection.BestCandidate.Box.Width, cardDetection.BestCandidate.Box.Height,
                cardDetection.BestCandidate.Score, cardDetection.AreaRatio);

            // 3. Phát hiện khuôn mặt trên ảnh chân dung (Selfie)
            var selfieDetection = DetectFaceDetailed(selfieImage);
            if (selfieDetection.BestCandidate == null)
            {
                _logger.LogWarning("[DEBUG] No face found in selfie image");
                return new FaceMatchResultDto
                {
                    IsMatch = false,
                    Similarity = 0,
                    IsBothFaceFound = false,
                    Message = "Không tìm thấy khuôn mặt trong ảnh chân dung. Vui lòng chụp rõ mặt trong điều kiện đủ sáng."
                };
            }

            _logger.LogInformation("[DEBUG] Selfie face detected: Box={X},{Y},{W},{H} Score={S:F3} AreaRatio={A:F4} Count={C}",
                selfieDetection.BestCandidate.Box.X, selfieDetection.BestCandidate.Box.Y,
                selfieDetection.BestCandidate.Box.Width, selfieDetection.BestCandidate.Box.Height,
                selfieDetection.BestCandidate.Score, selfieDetection.AreaRatio, selfieDetection.TotalCandidates);

            // 4. Cảnh báo nếu phát hiện nhiều khuôn mặt (nhưng vẫn tiếp tục so khớp với best candidate)
            if (selfieDetection.TotalCandidates > 1)
            {
                _logger.LogWarning("[DEBUG] Multiple faces detected in selfie: {Count}. Proceeding with best candidate.",
                    selfieDetection.TotalCandidates);
            }

            if (selfieDetection.AreaRatio < 0.025)
            {
                return new FaceMatchResultDto
                {
                    IsMatch = false,
                    Similarity = 0,
                    IsBothFaceFound = false,
                    Message = "Khuôn mặt trong ảnh chân dung quá xa. Vui lòng đưa máy lại gần hơn để nhận diện rõ nét."
                };
            }

            // 5. Cắt và chuẩn hóa khuôn mặt dạng vuông (Square Crop 35% Margin cân đối)
            using var cardFace = CropAndPrepareFace(cardImage, cardDetection.BestCandidate.Box);
            using var selfieFace = CropAndPrepareFace(selfieImage, selfieDetection.BestCandidate.Box);

            _logger.LogInformation("[DEBUG] Cropped card face: {W}x{H}, Cropped selfie face: {SW}x{SH}",
                cardFace.Width, cardFace.Height, selfieFace.Width, selfieFace.Height);

            // 6. Trích xuất vector đặc trưng (InsightFace ArcFace Embeddings)
            var cardEmbedding = ExtractEmbedding(cardFace);
            var selfieEmbedding = ExtractEmbedding(selfieFace);

            _logger.LogInformation("[DEBUG] Card embedding: Len={CLen}, Norm={CNorm:F4}, Min={CMin:F4}, Max={CMax:F4}, Mean={CMean:F6}",
                cardEmbedding.Length,
                MathF.Sqrt(cardEmbedding.Sum(v => v * v)),
                cardEmbedding.Length > 0 ? cardEmbedding.Min() : 0,
                cardEmbedding.Length > 0 ? cardEmbedding.Max() : 0,
                cardEmbedding.Length > 0 ? cardEmbedding.Average() : 0);
            _logger.LogInformation("[DEBUG] Selfie embedding: Len={SLen}, Norm={SNorm:F4}, Min={SMin:F4}, Max={SMax:F4}, Mean={SMean:F6}",
                selfieEmbedding.Length,
                MathF.Sqrt(selfieEmbedding.Sum(v => v * v)),
                selfieEmbedding.Length > 0 ? selfieEmbedding.Min() : 0,
                selfieEmbedding.Length > 0 ? selfieEmbedding.Max() : 0,
                selfieEmbedding.Length > 0 ? selfieEmbedding.Average() : 0);

            // 7. Tính khoảng cách Cosine Similarity
            var rawSimilarity = ComputeCosineSimilarity(cardEmbedding, selfieEmbedding);

            // 8. Quy đổi sang thang điểm chuẩn eKYC (0 - 100%)
            var similarityPercent = ConvertToPercentage(rawSimilarity);
            var isMatch = rawSimilarity >= MatchCosineThreshold || similarityPercent >= MatchPercentThreshold;

            _logger.LogInformation(
                "[DEBUG] Face Match: RawCos={RawCos:F6}, Score={Score:F1}%, Match={Match}",
                rawSimilarity, similarityPercent, isMatch);

            return new FaceMatchResultDto
            {
                IsMatch = isMatch,
                Similarity = Math.Round(similarityPercent, 1),
                IsBothFaceFound = true,
                Message = isMatch
                    ? $"Khuôn mặt trùng khớp với CCCD ({similarityPercent:F1}%)."
                    : $"Khuôn mặt không trùng khớp với CCCD ({similarityPercent:F1}%). Vui lòng chụp lại ở nơi đủ sáng."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi trong quá trình so khớp khuôn mặt");
            return new FaceMatchResultDto
            {
                IsMatch = false,
                Similarity = 0,
                IsBothFaceFound = false,
                Message = "Đã xảy ra lỗi khi xử lý hình ảnh: " + ex.Message
            };
        }
    }

    #region AI Detection & Recognition Helpers

    private DetectionResult DetectFaceDetailed(Image<Rgb24> image)
    {
        if (_detectorSession == null)
            return new DetectionResult(null, 0, 0, false);

        int origWidth = image.Width;
        int origHeight = image.Height;

        // Direct Resize về 320x240 (đúng cách UltraFace được huấn luyện)
        // KHÔNG dùng Letterboxing vì UltraFace priors được calibrate cho full-frame resize
        using var resized = image.Clone(ctx => ctx.Resize(DetectorInputWidth, DetectorInputHeight));

        // Create Tensor (1, 3, 240, 320) normalized (x - 127.0) / 128.0
        var inputTensor = new DenseTensor<float>(new[] { 1, 3, DetectorInputHeight, DetectorInputWidth });
        for (int y = 0; y < DetectorInputHeight; y++)
        {
            for (int x = 0; x < DetectorInputWidth; x++)
            {
                var pixel = resized[x, y];
                inputTensor[0, 0, y, x] = (pixel.R - 127.0f) / 128.0f;
                inputTensor[0, 1, y, x] = (pixel.G - 127.0f) / 128.0f;
                inputTensor[0, 2, y, x] = (pixel.B - 127.0f) / 128.0f;
            }
        }

        var inputMeta = _detectorSession.InputMetadata.Keys.First();
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputMeta, inputTensor) };

        using var results = _detectorSession.Run(inputs);
        var scoreResult = results.FirstOrDefault(r => r.Name.Contains("scores") || r.Name.Contains("output") || r.Name == "scores");
        var boxResult = results.FirstOrDefault(r => r.Name.Contains("boxes") || r.Name == "boxes");

        if (scoreResult == null || boxResult == null)
        {
            scoreResult = results.First();
            boxResult = results.Last();
        }

        var scoreTensor = scoreResult.AsTensor<float>();
        var boxTensor = boxResult.AsTensor<float>();

        var candidates = new List<FaceCandidate>();
        var totalPriors = _priors.Count;

        for (int i = 0; i < totalPriors; i++)
        {
            var faceScore = scoreTensor[0, i, 1];
            if (faceScore >= DetectorConfidenceThreshold)
            {
                var prior = _priors[i];
                var boxX = boxTensor[0, i, 0];
                var boxY = boxTensor[0, i, 1];
                var boxW = boxTensor[0, i, 2];
                var boxH = boxTensor[0, i, 3];

                // SSD decoding: offset → center coordinates (normalized 0-1)
                var cx = boxX * 0.1f * prior.W + prior.Cx;
                var cy = boxY * 0.1f * prior.H + prior.Cy;
                var w = MathF.Exp(boxW * 0.2f) * prior.W;
                var h = MathF.Exp(boxH * 0.2f) * prior.H;

                // Chuyển đổi tọa độ normalized → pixel coordinates gốc (tỷ lệ thuận)
                var x1 = Math.Clamp((cx - w / 2f) * origWidth, 0, origWidth);
                var y1 = Math.Clamp((cy - h / 2f) * origHeight, 0, origHeight);
                var x2 = Math.Clamp((cx + w / 2f) * origWidth, 0, origWidth);
                var y2 = Math.Clamp((cy + h / 2f) * origHeight, 0, origHeight);

                var rect = new Rectangle((int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));
                if (rect.Width > 10 && rect.Height > 10)
                {
                    candidates.Add(new FaceCandidate { Box = rect, Score = faceScore });
                }
            }
        }

        if (candidates.Count == 0)
            return new DetectionResult(null, 0, 0, false);

        // Apply Non-Maximum Suppression (NMS)
        var nmsBoxes = NonMaxSuppression(candidates, IouThreshold);
        var best = nmsBoxes.OrderByDescending(c => c.Score).FirstOrDefault();

        if (best == null)
            return new DetectionResult(null, 0, 0, false);

        double totalImageArea = (double)origWidth * origHeight;
        double faceArea = (double)best.Box.Width * best.Box.Height;
        double areaRatio = faceArea / totalImageArea;

        bool isTouchingBorder = best.Box.Left <= 4 ||
                                best.Box.Top <= 4 ||
                                best.Box.Right >= origWidth - 4 ||
                                best.Box.Bottom >= origHeight - 4;

        return new DetectionResult(best, nmsBoxes.Count, areaRatio, isTouchingBorder);
    }

    private Image<Rgb24> CropAndPrepareFace(Image<Rgb24> image, Rectangle box)
    {
        // Tạo khung vuông với 35% margin mở rộng quanh khuôn mặt
        int maxDim = Math.Max(box.Width, box.Height);
        int margin = (int)(maxDim * 0.35f);
        int squareSize = maxDim + margin * 2;

        int centerX = box.X + box.Width / 2;
        int centerY = box.Y + box.Height / 2;

        int x = Math.Max(0, centerX - squareSize / 2);
        int y = Math.Max(0, centerY - squareSize / 2);
        int w = Math.Min(image.Width - x, squareSize);
        int h = Math.Min(image.Height - y, squareSize);

        var cropRect = new Rectangle(x, y, w, h);
        return image.Clone(ctx => ctx
            .Crop(cropRect)
            .Resize(RecognizerInputSize, RecognizerInputSize));
    }

    private float[] ExtractEmbedding(Image<Rgb24> faceImage)
    {
        if (_recognizerSession == null) return Array.Empty<float>();

        // Chuẩn RGB cho InsightFace ArcFace: Channel 0=R, Channel 1=G, Channel 2=B
        var inputTensor = new DenseTensor<float>(new[] { 1, 3, RecognizerInputSize, RecognizerInputSize });
        for (int y = 0; y < RecognizerInputSize; y++)
        {
            for (int x = 0; x < RecognizerInputSize; x++)
            {
                var pixel = faceImage[x, y];
                inputTensor[0, 0, y, x] = (pixel.R - 127.5f) / 127.5f; // R
                inputTensor[0, 1, y, x] = (pixel.G - 127.5f) / 127.5f; // G
                inputTensor[0, 2, y, x] = (pixel.B - 127.5f) / 127.5f; // B
            }
        }

        var inputMeta = _recognizerSession.InputMetadata.Keys.First();
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputMeta, inputTensor) };

        using var results = _recognizerSession.Run(inputs);
        var outputTensor = results.First().AsTensor<float>();

        var embedding = outputTensor.ToArray();

        // Chuẩn hóa L2 Norm
        var norm = MathF.Sqrt(embedding.Sum(v => v * v));
        if (norm > 1e-6f)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= norm;
            }
        }

        return embedding;
    }

    private static float ComputeCosineSimilarity(float[] v1, float[] v2)
    {
        if (v1.Length == 0 || v2.Length == 0 || v1.Length != v2.Length) return 0f;

        float dot = 0f;
        for (int i = 0; i < v1.Length; i++)
        {
            dot += v1[i] * v2[i];
        }
        return dot;
    }

    private static double ConvertToPercentage(float cosineSim)
    {
        // Hiệu chuẩn thang điểm eKYC:
        // Raw cosine < 0.05: Người khác nhau hoàn toàn (0 - 10%)
        // Raw cosine 0.05 - 0.22: Khác biệt đáng kể (10 - 68%)
        // Raw cosine 0.22 - 0.55: Cùng một người (72 - 94%)
        // Raw cosine > 0.55: Trùng khớp rất cao (94 - 99.5%)
        if (cosineSim <= 0.05f)
        {
            return Math.Max(0.0, cosineSim * 200.0);
        }
        if (cosineSim < 0.22f)
        {
            return 10.0 + (cosineSim - 0.05f) / (0.22f - 0.05f) * 58.0;
        }
        if (cosineSim < 0.55f)
        {
            return 72.0 + (cosineSim - 0.22f) / (0.55f - 0.22f) * 22.0;
        }

        return Math.Clamp(94.0 + (cosineSim - 0.55f) * 12.0, 94.0, 99.5);
    }

    private void GeneratePriors()
    {
        int[][] featureMaps = { new[] { 30, 40 }, new[] { 15, 20 }, new[] { 8, 10 }, new[] { 4, 5 } };
        int[] strides = { 8, 16, 32, 64 };
        int[][] minBoxes =
        {
            new[] { 10, 16, 24 },
            new[] { 32, 48 },
            new[] { 64, 96 },
            new[] { 128, 192, 256 }
        };

        _priors.Clear();

        for (int k = 0; k < featureMaps.Length; k++)
        {
            var fm = featureMaps[k];
            var stride = strides[k];
            var minBoxList = minBoxes[k];

            for (int i = 0; i < fm[0]; i++)
            {
                for (int j = 0; j < fm[1]; j++)
                {
                    foreach (var minBox in minBoxList)
                    {
                        var cx = (j + 0.5f) * stride / DetectorInputWidth;
                        var cy = (i + 0.5f) * stride / DetectorInputHeight;
                        var w = (float)minBox / DetectorInputWidth;
                        var h = (float)minBox / DetectorInputHeight;

                        _priors.Add(new Anchor { Cx = cx, Cy = cy, W = w, H = h });
                    }
                }
            }
        }
    }

    private static List<FaceCandidate> NonMaxSuppression(List<FaceCandidate> boxes, float iouThresh)
    {
        var result = new List<FaceCandidate>();
        var sorted = boxes.OrderByDescending(b => b.Score).ToList();

        while (sorted.Count > 0)
        {
            var best = sorted[0];
            result.Add(best);
            sorted.RemoveAt(0);

            sorted.RemoveAll(candidate => ComputeIou(best.Box, candidate.Box) > iouThresh);
        }

        return result;
    }

    private static float ComputeIou(Rectangle a, Rectangle b)
    {
        var x1 = Math.Max(a.Left, b.Left);
        var y1 = Math.Max(a.Top, b.Top);
        var x2 = Math.Min(a.Right, b.Right);
        var y2 = Math.Min(a.Bottom, b.Bottom);

        var intersectionArea = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
        var unionArea = a.Width * a.Height + b.Width * b.Height - intersectionArea;

        return unionArea <= 0 ? 0f : (float)intersectionArea / unionArea;
    }

    private static string ResolveModelPath(string fileName)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var p1 = Path.Combine(baseDir, "AI", "Models", fileName);
        if (File.Exists(p1)) return p1;

        var currentDir = Directory.GetCurrentDirectory();
        var p2 = Path.Combine(currentDir, "Infrastructure", "AI", "Models", fileName);
        if (File.Exists(p2)) return p2;

        var p3 = Path.Combine(currentDir, "..", "Infrastructure", "AI", "Models", fileName);
        if (File.Exists(p3)) return Path.GetFullPath(p3);

        return p1;
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _detectorSession?.Dispose();
        _recognizerSession?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private record struct Anchor(float Cx, float Cy, float W, float H);
    private record DetectionResult(FaceCandidate? BestCandidate, int TotalCandidates, double AreaRatio, bool IsTouchingBorder);
    private class FaceCandidate
    {
        public Rectangle Box { get; set; }
        public float Score { get; set; }
    }
}
