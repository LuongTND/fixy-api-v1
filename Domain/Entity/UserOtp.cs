using Domain.Common;

namespace Domain.Entity
{
    public class UserOtp : BaseEntity
    {
        public string Target { get; set; } = default!;

        // phone number OR email

        public string OtpCode { get; set; } = default!;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public bool IsVerified { get; set; }
    }
}
