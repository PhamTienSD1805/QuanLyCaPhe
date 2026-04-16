using System.Security.Cryptography;

namespace QuanLyQuanCaPhe.Utils
{
    /// <summary>
    /// Tiện ích sinh và kiểm tra OTP 6 chữ số.
    /// Dùng RandomNumberGenerator để đảm bảo tính bảo mật (cryptographically secure).
    /// </summary>
    public static class OtpUtil
    {
        private const int OtpLength      = 6;
        private const int ExpiryMinutes  = 5;   // OTP hết hạn sau 5 phút

        // ----------------------------------------------------------------
        // Generate — sinh OTP ngẫu nhiên an toàn
        // ----------------------------------------------------------------

        /// <summary>
        /// Tạo OTP gồm 6 chữ số (000000–999999).
        /// Dùng RandomNumberGenerator (CSPRNG) thay vì Random thông thường.
        /// </summary>
        public static string Generate()
        {
            // Sinh số ngẫu nhiên trong khoảng [0, 1_000_000)
            int number = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return number.ToString($"D{OtpLength}");  // padding 0 cho đủ 6 chữ số
        }

        // ----------------------------------------------------------------
        // Expiry helpers
        // ----------------------------------------------------------------

        /// <summary>Thời điểm hết hạn = hiện tại + 5 phút.</summary>
        public static DateTime GetExpiry() => DateTime.Now.AddMinutes(ExpiryMinutes);

        /// <summary>Số phút OTP còn hiệu lực (dùng cho email template).</summary>
        public static int ExpiryMinutesValue => ExpiryMinutes;

        /// <summary>Kiểm tra OTP đã hết hạn chưa.</summary>
        public static bool IsExpired(DateTime expiresAt) => DateTime.Now > expiresAt;
    }
}
