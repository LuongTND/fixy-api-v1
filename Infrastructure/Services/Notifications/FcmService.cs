using Application.Interfaces.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Notifications
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _isInitialized;

        public FcmService(IConfiguration configuration,ILogger<FcmService> logger,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            // Khởi tạo FirebaseApp Singleton nếu chưa tồn tại
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialJson = configuration["Firebase:CredentialJson"];
                var jsonPath = configuration["Firebase:CredentialJsonPath"];

                try
                {
                    GoogleCredential credential;

                    if (!string.IsNullOrEmpty(credentialJson))
                    {
                        // Ưu tiên load từ biến môi trường (Production)
                        credential = GoogleCredential.FromJson(credentialJson);
                    }
                    else if (!string.IsNullOrEmpty(jsonPath))
                    {
                        // Load từ file path (Development)
                        credential = GoogleCredential.FromFile(jsonPath);
                    }
                    else
                    {
                        _logger.LogWarning("Firebase credentials not configured. FCM push notifications will be disabled.");
                        _isInitialized = false;
                        return;
                    }

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });

                    _isInitialized = true;
                    _logger.LogInformation("Firebase App initialized successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Firebase App. FCM push notifications will be disabled.");
                    _isInitialized = false;
                }
            }
            else
            {
                _isInitialized = true;
            }
        }

        public async Task<bool> SendPushNotificationAsync(string token,string title,string body,string? deepLink = null,object? meta = null,CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Firebase not initialized. Skipping push notification.");
                return false;
            }

            try
            {
                var dataPayload = new Dictionary<string, string>
                {
                    { "title", title },
                    { "body", body },
                    { "deepLink", deepLink ?? "" }
                };

                if (meta != null)
                {
                    dataPayload.Add("meta", JsonSerializer.Serialize(meta, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                }

                var message = new Message
                {
                    Token = token,
                    Data = dataPayload
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
                _logger.LogInformation("FCM data-only push notification sent. MessageId: {MessageId}", response);
                return true;
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("FCM token {Token} is no longer registered. Removing stale token.", token);
                await RemoveInvalidTokenAsync(token);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send FCM push notification to token {Token}", token);
                return false;
            }
        }

        public async Task SendBatchPushNotificationAsync(List<string> tokens,string title,string body, string? deepLink = null,object? meta = null, CancellationToken cancellationToken = default)
        {
            if (!_isInitialized || tokens == null || tokens.Count == 0) return;

            var dataPayload = new Dictionary<string, string>
            {
                { "title", title },
                { "body", body },
                { "deepLink", deepLink ?? "" }
            };

            if (meta != null)
            {
                dataPayload.Add("meta", JsonSerializer.Serialize(meta, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }

            // Firebase giới hạn 500 messages mỗi batch call
            const int batchLimit = 500;

            for (int i = 0; i < tokens.Count; i += batchLimit)
            {
                var currentBatch = tokens.Skip(i).Take(batchLimit).ToList();
                var messages = currentBatch.Select(t => new Message
                {
                    Token = t,
                    Data = dataPayload
                }).ToList();

                try
                {
                    var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages, cancellationToken);
                    _logger.LogInformation(
                        "FCM batch sent: {SuccessCount} success, {FailureCount} failure out of {Total}.",
                        response.SuccessCount, response.FailureCount, currentBatch.Count);

                    // Xử lý và dọn dẹp các token không còn hợp lệ
                    var invalidTokens = new List<string>();
                    for (int j = 0; j < response.Responses.Count; j++)
                    {
                        var sendResponse = response.Responses[j];
                        if (!sendResponse.IsSuccess &&
                            sendResponse.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered)
                        {
                            invalidTokens.Add(currentBatch[j]);
                        }
                    }

                    if (invalidTokens.Count > 0)
                    {
                        _logger.LogWarning("Found {Count} stale FCM tokens to remove.", invalidTokens.Count);
                        await RemoveInvalidTokensAsync(invalidTokens);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send FCM batch push notifications for batch starting at index {Index}.", i);
                }
            }
        }

        private async Task RemoveInvalidTokenAsync(string token)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Persistence.AppDbContext>();
                var tokenEntity = context.UserFcmTokens.FirstOrDefault(t => t.Token == token);
                if (tokenEntity != null)
                {
                    context.UserFcmTokens.Remove(tokenEntity);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Removed stale FCM token from database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove stale FCM token.");
            }
        }

        private async Task RemoveInvalidTokensAsync(List<string> tokens)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Persistence.AppDbContext>();
                var staleTokens = context.UserFcmTokens
                    .Where(t => tokens.Contains(t.Token))
                    .ToList();

                if (staleTokens.Count > 0)
                {
                    context.UserFcmTokens.RemoveRange(staleTokens);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Removed {Count} stale FCM tokens from database.", staleTokens.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove stale FCM tokens in batch.");
            }
        }
    }
}
