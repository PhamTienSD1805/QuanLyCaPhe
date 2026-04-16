using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCaPhe.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email    { get; set; } = string.Empty;

        /// <summary>Để trống khi edit = giữ nguyên mật khẩu cũ.</summary>
        [Display(Name = "Mật khẩu")]
        [MinLength(4, ErrorMessage = "Mật khẩu tối thiểu 4 ký tự")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Vai trò")]
        public bool Role { get; set; }   // true = Manager

        [Display(Name = "Kích hoạt")]
        public bool Active { get; set; } = true;
    }
}
