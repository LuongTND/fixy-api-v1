using System;
using Domain.Enum;

namespace Application.DTOs.VoucherCampaign
{
    public class UpdateCampaignDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public long? BudgetLimit { get; set; }
        public VoucherEventType? AutoTriggerEvent { get; set; }
    }
}
