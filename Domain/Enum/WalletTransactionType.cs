namespace Domain.Enum
{
    public enum WalletTransactionType
    {
        TopUp, // nạp tiền
        Payment, // khách trả tiền đơn hàng
        Refund, // hoàn tiền
        Commission, // hoa hồng hệ thống lấy
        BookingIncome, // cộng tiền vào ví thợ
        Withdrawal, // thợ rút tiền
        Fee, // phí nền tảng (service fee)
        Adjustment, // admin chỉnh tay
    }
}
