namespace AMPFashionStore.Services
{
    /// <summary>
    /// Thông tin tài khoản ngân hàng dùng để tạo mã QR thanh toán (theo chuẩn VietQR.io).
    /// BankId là mã BIN ngân hàng (vd: 970436 = Vietcombank, 970422 = MB Bank...).
    /// </summary>
    public class BankSettings
    {
        public string BankId { get; set; } = "970436";
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Template { get; set; } = "compact2";

        /// <summary>Sinh link ảnh QR động theo số tiền và nội dung chuyển khoản.</summary>
        public string TaoLinkQr(decimal soTien, string noiDung)
        {
            var amount = ((long)soTien).ToString();
            var info = Uri.EscapeDataString(noiDung);
            var accName = Uri.EscapeDataString(AccountName);
            return $"https://img.vietqr.io/image/{BankId}-{AccountNumber}-{Template}.png?amount={amount}&addInfo={info}&accountName={accName}";
        }
    }
}
