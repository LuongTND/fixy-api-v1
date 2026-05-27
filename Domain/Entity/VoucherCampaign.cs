using System;
using System.Collections.Generic;
using Domain.Common;
using Domain.Enum;

namespace Domain.Entity
{
    public class VoucherCampaign : BaseEntity, ISoftDelete
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        public long? BudgetLimit { get; set; } // Limit total budget  (VND)
        public long BudgetUsed { get; set; }   // Budget spend (VND)
        
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
        
        public VoucherEventType? AutoTriggerEvent { get; set; }

        // ISoftDelete Implementation
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation properties
        public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
    }
}
