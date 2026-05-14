namespace Application.Common.Models.Payment
{
    public class MoMoCreatePaymentRequest
    {
        public string PartnerCode { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string RequestId { get; set; } = default!;
        public long Amount { get; set; }
        public string OrderId { get; set; } = default!;
        public string OrderInfo { get; set; } = default!;
        public string ReturnUrl { get; set; } = default!;
        public string NotifyUrl { get; set; } = default!;
        public string ExtraData { get; set; } = "";
        public string RequestType { get; set; } = "captureWallet";
        public string Signature { get; set; } = default!;
    }
}
