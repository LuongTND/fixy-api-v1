using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace API.Extensions
{
    public static class RateLimiterExtensions
    {
        public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Return 429 Too Many Requests when rate limit is exceeded
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Register rate limiting policies
                ConfigureBookingDraftPolicy(options);
            });

            return services;
        }

        /// <summary>
        /// Limits booking draft creation to 5 requests per minute per user.
        /// Prevents spam creation of drafts that consume Redis cache resources.
        /// </summary>
        private static void ConfigureBookingDraftPolicy(RateLimiterOptions options)
        {
            options.AddPolicy("BookingDraftPolicy", context =>
            {
                var userId = context.User.Identity?.Name ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: userId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        }
    }
}
