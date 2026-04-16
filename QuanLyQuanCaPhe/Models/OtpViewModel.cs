using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCaPhe.Models
{
    // ====================================================================
    // Bước 1: Nhập email → gửi OTP
    // ====================================================================
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    // ====================================================================
    // Bước 2: Nhập mã OTP 6 chữ số
    // ====================================================================
    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải gồm đúng 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP chỉ gồm 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = string.Empty;
    }

    // ====================================================================
    // Bước 3: Nhập mật khẩu mới
    // ====================================================================
    public class ResetPasswordViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        // Token xác nhận OTP đã verify (int của OTP id)
        [Required]
        public int OtpId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(4, ErrorMessage = "Mật khẩu tối thiểu 4 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // Kept for backward compat (ForgotPasswordOtp, etc. removed from flow)
    public class ForgotPasswordOtpViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}
