using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace AMPFashionStore.Services
{
    /// <summary>
    /// Gửi email OTP qua SMTP (mặc định cấu hình cho Gmail).
    /// Dùng System.Net.Mail có sẵn trong .NET - không cần cài thêm gói NuGet nào.
    /// Nếu chưa cấu hình SMTP (SenderEmail rỗng) hoặc gửi lỗi, service sẽ log lại và
    /// trả về false thay vì làm crash luồng xử lý - AccountController sẽ tự động
    /// hiển thị mã OTP trực tiếp lên giao diện ở môi trường Development để tiện demo/chấm bài.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> GuiOtpAsync(string toEmail, string hoTen, string maOtp, string mucDich)
        {
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.SenderPassword))
            {
                _logger.LogWarning("EmailSettings chưa được cấu hình trong appsettings.json. Mã OTP cho {Email}: {Otp}", toEmail, maOtp);
                return false;
            }

            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                    EnableSsl = _settings.EnableSsl
                };

                var subject = $"[AMP Fashion Store] Mã xác thực {mucDich}";
                var body = $@"
                    <div style='font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #eee;padding:32px'>
                        <h2 style='color:#6B2337;margin-bottom:4px'>AMP FASHION STORE</h2>
                        <p>Xin chào {WebUtility.HtmlEncode(hoTen)},</p>
                        <p>Bạn (hoặc ai đó) vừa yêu cầu mã xác thực cho chức năng <b>{WebUtility.HtmlEncode(mucDich)}</b>.</p>
                        <p style='margin:24px 0'>Mã OTP của bạn là:</p>
                        <div style='font-size:32px;font-weight:700;letter-spacing:8px;color:#1A1A1A;background:#F7F5F2;padding:16px;text-align:center;border-radius:4px'>{maOtp}</div>
                        <p style='margin-top:24px;color:#666'>Mã có hiệu lực trong 5 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                        <p style='color:#666'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.</p>
                    </div>";

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gửi email OTP thất bại cho {Email}. Mã OTP: {Otp}", toEmail, maOtp);
                return false;
            }
        }
    }
}
