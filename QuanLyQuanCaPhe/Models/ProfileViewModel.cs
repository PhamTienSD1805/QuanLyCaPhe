using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCaPhe.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;   // read-only

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        // ------- Đổi mật khẩu (không bắt buộc) -------
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPassword { get; set; }

        [MinLength(4, ErrorMessage = "Mật khẩu tối thiểu 4 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string? NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string? ConfirmPassword { get; set; }

        public string Role => string.Empty; // filled in controller for display
        public bool   Active { get; set; }
    }
}
