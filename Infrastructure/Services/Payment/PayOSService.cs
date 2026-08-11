using Application.Interfaces.Services.Payment;
using Application.Settings;
using Domain.Entity;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;

namespace Infrastructure.Services.Payment;

public class PayOSService : IPaymentGateway
{
    private readonly PayOS _payOS;

    private readonly PayOSSettings _options;

    public PayOSService(IOptions<PayOSSettings> options)
    {
        _options = options.Value;

        _payOS = new PayOS(_options.ClientId, _options.ApiKey, _options.ChecksumKey);
    }

    public async Task<string> CreatePaymentUrlAsync(
        PaymentOrder order,
        CancellationToken cancellationToken
    )
    {
        long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        order.GatewayOrderCode = orderCode;
        var items = new List<ItemData>
        {
            new ItemData(name: order.Type.ToString(), quantity: 1, price: (int)order.FinalAmount),
        };

        var paymentData = new PaymentData(
            orderCode: orderCode,
            amount: (int)order.FinalAmount,
            description: $"Order_{order.Id.ToString().Split('-')[0]}",
            items: items,
            cancelUrl: _options.CancelUrl,
            returnUrl: _options.ReturnUrl
        );

        var response = await _payOS.createPaymentLink(paymentData);

        order.ExternalTransactionId = orderCode.ToString();

        return response.checkoutUrl;
    }

    /// <summary>
    /// Verify the PayOS webhook signature using the SDK's built-in checksum verification.
    /// Throws an exception if the signature is invalid.
    /// </summary>
    public WebhookData VerifyWebhookData(WebhookType webhookBody)
    {
        return _payOS.verifyPaymentWebhookData(webhookBody);
    }

    public bool VerifySignature(Dictionary<string, string> response)
    {
        // PayOS uses webhook-based verification via VerifyWebhookData(),
        // not the query-string approach used by VNPay/MoMo.
        return true;
    }
}

