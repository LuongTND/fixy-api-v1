namespace Application.DTOs.WorkerProfile
{
    public class WorkerDepositStatusDto
    {
        public Guid? PrimaryServiceId { get; set; }

        public string? PrimaryServiceName { get; set; }

        public string? ServiceCategoryName { get; set; }

        public long DepositRequiredAmount { get; set; } // Bằng BasePrice dịch vụ Spa chính

        public long LockedBalance { get; set; } // Tiền cọc hiện có trong ví

        public long AvailableBalance { get; set; } // Thu nhập khả dụng để rút

        public bool IsDepositPaid { get; set; } // LockedBalance >= DepositRequiredAmount

        public DateTime? DepositPaidAt { get; set; }

        public bool CanRequestRefund { get; set; } // Đủ điều kiện rút cọc (không còn ca dở dang)
    }
}
