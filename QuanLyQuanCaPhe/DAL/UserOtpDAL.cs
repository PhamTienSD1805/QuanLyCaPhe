using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.DAL
{
    /// <summary>
    /// Data Access Layer cho bảng user_otps.
    /// Xử lý tạo OTP, cooldown 60 giây, truy vấn và đánh dấu đã sử dụng.
    /// </summary>
    public class UserOtpDAL
    {
        private const int CooldownSeconds = 60;  // anti-spam: chờ 60 giây giữa 2 lần gửi

        // ----------------------------------------------------------------
        // CREATE — lưu OTP mới vào DB (kèm created_at)
        // ----------------------------------------------------------------

        /// <summary>
        /// Tạo một bản ghi OTP mới cho user.
        /// Trả về id của OTP vừa tạo.
        /// </summary>
        public static int Create(int userId, string otpCode, DateTime createdAt, DateTime expiresAt)
        {
            var otp = new UserOtp
            {
                UserId    = userId,
                OtpCode   = otpCode,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt
            };
            return otp.Insert();
        }

        // ----------------------------------------------------------------
        // READ — lấy OTP mới nhất (chưa dùng) của user
        // ----------------------------------------------------------------

        /// <summary>
        /// Trả về OTP chưa dùng mới nhất của user (kể cả đã hết hạn).
        /// Dùng để kiểm tra cooldown và validate.
        /// </summary>
        public static UserOtp? GetLatest(int userId)
            => UserOtp.GetLatestByUser(userId);

        // ----------------------------------------------------------------
        // COOLDOWN — chống spam gửi OTP liên tục
        // ----------------------------------------------------------------

        /// <summary>
        /// Kiểm tra user có đang trong thời gian cooldown không.
        /// Trả về (isInCooldown, secondsRemaining).
        /// </summary>
        public static (bool IsInCooldown, int SecondsRemaining) CheckCooldown(int userId)
        {
            var latest = GetLatest(userId);
            if (latest == null) return (false, 0);

            int remaining = latest.GetCooldownSecondsRemaining(CooldownSeconds);
            return (remaining > 0, remaining);
        }

        // ----------------------------------------------------------------
        // UPDATE — đánh dấu OTP đã sử dụng
        // ----------------------------------------------------------------

        /// <summary>
        /// Đánh dấu OTP đã được sử dụng (used = 1).
        /// Trả về true nếu cập nhật thành công.
        /// </summary>
        public static bool MarkUsed(int otpId)
            => UserOtp.MarkUsed(otpId);

        // ----------------------------------------------------------------
        // VALIDATE — kiểm tra OTP hợp lệ để đặt lại mật khẩu
        // ----------------------------------------------------------------

        /// <summary>
        /// Kiểm tra OTP nhập vào có hợp lệ không:
        /// 1. OTP tồn tại trong DB (chưa dùng)
        /// 2. Chưa hết hạn (expires_at > NOW)
        /// 3. Mã khớp với <paramref name="otpCode"/>
        /// </summary>
        public static (bool IsValid, UserOtp? Otp, string ErrorMessage) Validate(int userId, string otpCode)
        {
            var otp = GetLatest(userId);

            if (otp == null)
                return (false, null, "Không tìm thấy mã OTP. Vui lòng yêu cầu lại.");

            if (otp.Used)
                return (false, otp, "Mã OTP đã được sử dụng. Vui lòng yêu cầu mã mới.");

            if (OtpUtil.IsExpired(otp.ExpiresAt))
                return (false, otp, "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");

            if (otp.OtpCode != otpCode.Trim())
                return (false, otp, "Mã OTP không đúng. Vui lòng kiểm tra lại.");

            return (true, otp, string.Empty);
        }
    }
}
