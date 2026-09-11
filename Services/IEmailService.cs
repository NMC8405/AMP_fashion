namespace AMPFashionStore.Services
{
    public interface IEmailService
    {
        /// <summary>Gửi email chứa mã OTP. Trả về true nếu gửi thành công qua SMTP.</summary>
        Task<bool> GuiOtpAsync(string toEmail, string hoTen, string maOtp, string mucDich);
    }
}
