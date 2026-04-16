using System.Data;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.Models
{
    /// <summary>
    /// Entity ánh xạ bảng user_otps trong SQL Server.
    /// Mỗi bản ghi lưu 1 OTP 6 chữ số kèm thời gian tạo, hết hạn và trạng thái đã dùng.
    /// </summary>
    public class UserOtp
    {
        public int      Id        { get; set; }
        public int      UserId    { get; set; }
        public string   OtpCode   { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }   // ← thêm mới (cooldown)
        public DateTime ExpiresAt { get; set; }
        public bool     Used      { get; set; }

        // ----------------------------------------------------------------
        // Mapping helper
        // ----------------------------------------------------------------
        private static UserOtp FromRow(DataRow row) => new()
        {
            Id        = Convert.ToInt32(row["id"]),
            UserId    = Convert.ToInt32(row["user_id"]),
            OtpCode   = row["otp_code"].ToString()!,
            CreatedAt = Convert.ToDateTime(row["created_at"]),
            ExpiresAt = Convert.ToDateTime(row["expires_at"]),
            Used      = Convert.ToBoolean(row["used"])
        };

        // ----------------------------------------------------------------
        // INSERT — lưu OTP mới, trả về id vừa tạo
        // ----------------------------------------------------------------
        public int Insert()
        {
            const string sql = @"INSERT INTO user_otps (user_id, otp_code, created_at, expires_at, used)
                                 VALUES (@1, @2, @3, @4, 0);
                                 SELECT SCOPE_IDENTITY();";
            var result = DBUtil.ExecuteScalar(sql, UserId, OtpCode, CreatedAt, ExpiresAt);
            return Convert.ToInt32(result);
        }

        // ----------------------------------------------------------------
        // GET — OTP mới nhất (chưa dùng) của user, kể cả đã hết hạn
        // ----------------------------------------------------------------
        public static UserOtp? GetLatestByUser(int userId)
        {
            const string sql = @"SELECT TOP 1 id, user_id, otp_code, created_at, expires_at, used
                                 FROM user_otps
                                 WHERE user_id = @1 AND used = 0
                                 ORDER BY created_at DESC";
            var dt = DBUtil.QueryDataTable(sql, userId);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        // ----------------------------------------------------------------
        // MARK USED — đánh dấu OTP đã sử dụng
        // ----------------------------------------------------------------
        public static bool MarkUsed(int id)
        {
            const string sql = "UPDATE user_otps SET used = 1 WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }

        // ----------------------------------------------------------------
        // Cooldown helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Tính số giây còn lại trong thời gian chờ (cooldown 60 giây).
        /// Trả về 0 nếu đã hết cooldown.
        /// </summary>
        public int GetCooldownSecondsRemaining(int cooldownSeconds = 60)
        {
            var elapsed = (int)(DateTime.Now - CreatedAt).TotalSeconds;
            return Math.Max(0, cooldownSeconds - elapsed);
        }

        /// <summary>Kiểm tra OTP này có đang trong cooldown không.</summary>
        public bool IsInCooldown(int cooldownSeconds = 60)
            => GetCooldownSecondsRemaining(cooldownSeconds) > 0;
    }
}
