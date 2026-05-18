using System.Net;
using Application.Interfaces.Services.Payment;
using Application.Settings;
using Domain.Entity;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Payment
{
    public class VnPayService : IPaymentGateway
    {
        private readonly VNPaySettings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VnPayService(
            IOptions<VNPaySettings> settings,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _settings = settings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<string> CreatePaymentUrlAsync(
            PaymentOrder order,
            CancellationToken cancellationToken
        )
        {
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            );

            var request = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _settings.TmnCode },
                { "vnp_Amount", (order.FinalAmount * 100).ToString() },
                { "vnp_CreateDate", vnTime.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", VnPayHelper.GetIpAddress(_httpContextAccessor.HttpContext!) },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Topup_{order.Id:N}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", _settings.ReturnUrl },
                { "vnp_TxnRef", order.Id.ToString("N") },
            };

            var query = string.Join(
                "&",
                request.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}")
            );

            var secureHash = VnPayHelper.HmacSHA512(_settings.HashSecret, query);

            var paymentUrl = $"{_settings.BaseUrl}?{query}&vnp_SecureHash={secureHash}";

            return Task.FromResult(paymentUrl);
        }

        public bool VerifySignature(Dictionary<string, string> response)
        {
            if (!response.TryGetValue("vnp_SecureHash", out var secureHash))
                return false;

            var filtered = response
                .Where(x => x.Key != "vnp_SecureHash" && x.Key != "vnp_SecureHashType")
                .OrderBy(x => x.Key);

            var rawData = string.Join(
                "&",
                filtered.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}")
            );

            var checkHash = VnPayHelper.HmacSHA512(_settings.HashSecret, rawData);

            return checkHash.Equals(secureHash, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
