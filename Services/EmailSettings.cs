namespace AMPFashionStore.Services
{
    /// <summary>Cấu hình SMTP đọc từ appsettings.json (khóa "EmailSettings").</summary>
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public string SenderName { get; set; } = "AMP Fashion Store";
        public bool EnableSsl { get; set; } = true;
    }
}
