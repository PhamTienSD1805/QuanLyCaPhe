using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace QuanLyQuanCaPhe.Utils
{
    /// <summary>
    /// Utility class gửi email qua Gmail SMTP dùng MailKit.
    /// Cấu hình đọc từ EmailSettings trong appsettings.json.
    /// Gọi EmailUtil.Configure(config) một lần trong Program.cs.
    /// </summary>
    public static class EmailUtil
    {
        // ----------------------------------------------------------------
        // Config — được inject từ Program.cs
        // ----------------------------------------------------------------
        private static string _smtpHost     = "smtp.gmail.com";
        private static int    _smtpPort     = 587;
        private static string _senderEmail  = string.Empty;
        private static string _senderName   = "Cafe Manager";
        private static string _appPassword  = string.Empty;

        /// <summary>Gọi 1 lần trong Program.cs để nạp cấu hình email.</summary>
        public static void Configure(IConfiguration config)
        {
            var section = config.GetSection("EmailSettings");
            _smtpHost    = section["SmtpHost"]    ?? _smtpHost;
            _smtpPort    = int.Parse(section["SmtpPort"] ?? "587");
            _senderEmail = section["SenderEmail"] ?? string.Empty;
            _senderName  = section["SenderName"]  ?? _senderName;
            _appPassword = section["AppPassword"] ?? string.Empty;
        }

        // ----------------------------------------------------------------
        // Send — gửi email HTML
        // ----------------------------------------------------------------

        /// <summary>
        /// Gửi 1 email với nội dung HTML đến địa chỉ <paramref name="toEmail"/>.
        /// </summary>
        /// <param name="toEmail">Địa chỉ người nhận</param>
        /// <param name="toName">Tên hiển thị người nhận</param>
        /// <param name="subject">Tiêu đề email</param>
        /// <param name="htmlBody">Nội dung HTML của email</param>
        public static async Task SendAsync(
            string toEmail,
            string toName,
            string subject,
            string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To  .Add(new MailboxAddress(toName,      toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            // Gmail dùng STARTTLS trên port 587
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_senderEmail, _appPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);
        }

        // ----------------------------------------------------------------
        // Template helpers — email đẹp hơn với HTML
        // ----------------------------------------------------------------

        /// <summary>Template email gửi mật khẩu mới.</summary>
        public static string BuildNewPasswordEmail(string fullName, string newPassword) =>
            $"""
            <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e0d0c0;border-radius:8px;overflow:hidden;">
              <div style="background:#6f4e37;padding:20px;text-align:center;">
                <h2 style="color:#fff;margin:0;">☕ Cafe Manager</h2>
              </div>
              <div style="padding:24px;">
                <p>Xin chào <strong>{fullName}</strong>,</p>
                <p>Mật khẩu mới của bạn là:</p>
                <div style="background:#f5f0eb;border-radius:6px;padding:12px 20px;text-align:center;font-size:24px;letter-spacing:4px;font-weight:bold;color:#6f4e37;">
                  {newPassword}
                </div>
                <p style="margin-top:16px;">Vui lòng đăng nhập và đổi mật khẩu ngay sau khi nhận được email này.</p>
                <hr style="border:none;border-top:1px solid #e0d0c0;margin:20px 0;"/>
                <p style="color:#999;font-size:12px;">Email này được gửi tự động, vui lòng không trả lời.</p>
              </div>
            </div>
            """;

        /// <summary>Template email gửi OTP.</summary>
        public static string BuildOtpEmail(string fullName, string otpCode, int expiryMinutes) =>
            $"""
            <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e0d0c0;border-radius:8px;overflow:hidden;">
              <div style="background:#6f4e37;padding:20px;text-align:center;">
                <h2 style="color:#fff;margin:0;">☕ Cafe Manager</h2>
              </div>
              <div style="padding:24px;">
                <p>Xin chào <strong>{fullName}</strong>,</p>
                <p>Mã OTP đặt lại mật khẩu của bạn là:</p>
                <div style="background:#f5f0eb;border-radius:6px;padding:16px;text-align:center;font-size:36px;letter-spacing:8px;font-weight:bold;color:#6f4e37;">
                  {otpCode}
                </div>
                <p style="margin-top:16px;color:#c0392b;">
                  ⏰ Mã này sẽ hết hạn sau <strong>{expiryMinutes} phút</strong>.
                </p>
                <p>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                <hr style="border:none;border-top:1px solid #e0d0c0;margin:20px 0;"/>
                <p style="color:#999;font-size:12px;">Email này được gửi tự động, vui lòng không trả lời.</p>
              </div>
            </div>
            """;
    }
}
