namespace SweetSoft.QLDA.Core.EnumHelper.Defines
{
    // Keep the values already used by TblThanhToan in QLDA4.
    public enum ThanhToanStatus : byte
    {
        [ERender("PAYMENT_UNPAID")]
        ChuaThanhToan = 0,

        [ERender("PAYMENT_OVERDUE")]
        TreHan = 1,

        [ERender("PAYMENT_PAID")]
        DaThanhToan = 2
    }
}
