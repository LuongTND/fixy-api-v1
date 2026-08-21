using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces.Services.Payment;
using Application.Settings;
using Domain.Entity;
using Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Payment
{
    public class MoMoService : IPaymentGateway
    {
        private readonly MoMoSettings _settings;
        private readonly ILogger<MoMoService> _logger;

        public MoMoService(IOptions<MoMoSettings> settings, ILogger<MoMoService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> CreatePaymentUrlAsync(
            PaymentOrder order,
            CancellationToken cancellationToken
        )
        {
            var requestId = Guid.NewGuid().ToString("N");
            var orderId = order.Id.ToString("N");
            var amountValue = order.FinalAmount;
            var amountStr = amountValue.ToString();
            var orderInfo = $"Thanh toan don hang Fixy {orderId}";
            var extraData = "";

            var rawSignature =
                $"accessKey={_settings.AccessKey}"
                + $"&amount={amountStr}"
                + $"&extraData={extraData}"
                + $"&ipnUrl={_settings.NotifyUrl}"
                + $"&orderId={orderId}"
                + $"&orderInfo={orderInfo}"
                + $"&partnerCode={_settings.PartnerCode}"
                + $"&redirectUrl={_settings.ReturnUrl}"
                + $"&requestId={requestId}"
                + $"&requestType=payWithMethod";

            var signature = MoMoHelper.HmacSHA256(_settings.SecretKey, rawSignature);

            var body = new
            {
                partnerCode = _settings.PartnerCode,
                partnerName = "Fixy",
                storeId = "FixyStore",
                requestId,
                amount = amountValue,
                orderId,
                orderInfo,
                redirectUrl = _settings.ReturnUrl,
                ipnUrl = _settings.NotifyUrl,
                lang = "vi",
                extraData,
                requestType = "payWithMethod",
                signature,
            };
            using var client = new HttpClient();

            var response = await client.PostAsJsonAsync(_settings.BaseUrl, body, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var json = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (!json.TryGetProperty("payUrl", out var payUrl))
            {
                _logger.LogError("MoMo payment creation failed. OrderId: {OrderId}, Response: {Response}", orderId, responseContent);
                throw new InvalidOperationException($"MoMo không thể tạo thanh toán. Vui lòng thử lại sau.");
            }
            return payUrl.GetString()!;
        }

        public bool VerifySignature(Dictionary<string, string> response)
        {
            if (!response.TryGetValue("signature", out var signature))
            {
                return false;
            }

            var rawHash =
                $"accessKey={_settings.AccessKey}"
                + $"&amount={response["amount"]}"
                + $"&extraData={response["extraData"]}"
                + $"&message={response["message"]}"
                + $"&orderId={response["orderId"]}"
                + $"&orderInfo={response["orderInfo"]}"
                + $"&orderType={response["orderType"]}"
                + $"&partnerCode={response["partnerCode"]}"
                + $"&payType={response["payType"]}"
                + $"&requestId={response["requestId"]}"
                + $"&responseTime={response["responseTime"]}"
                + $"&resultCode={response["resultCode"]}"
                + $"&transId={response["transId"]}";

            var checkSignature = MoMoHelper.HmacSHA256(_settings.SecretKey, rawHash);

            return checkSignature.Equals(signature, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
