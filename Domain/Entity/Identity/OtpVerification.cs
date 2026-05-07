using Domain.Common;
using Domain.Enum;

namespace Domain.Entity.Identity
{
    public class OtpVerification : BaseAuditableEntity
    {
        public string Target { get; set; } = default!;

        // phone number OR email

        public OtpType Type { get; set; }

        public string OtpCode { get; set; } = default!;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public bool IsVerified { get; set; }
    }
}
