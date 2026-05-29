using System;
using Domain.Enum;

namespace Application.DTOs.VoucherCampaign
{
    public class CampaignDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public long? BudgetLimit { get; set; }
        public long BudgetUsed { get; set; }
        public CampaignStatus Status { get; set; }
        public VoucherEventType? AutoTriggerEvent { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
