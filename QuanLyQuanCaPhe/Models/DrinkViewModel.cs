using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCaPhe.Models
{
    public class DrinkViewModel
    {
        public int     Id          { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int     CategoryId  { get; set; }

        [Required(ErrorMessage = "Tên đồ uống là bắt buộc")]
        [StringLength(200)]
        [Display(Name = "Tên đồ uống")]
        public string  Name        { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(1, 10_000_000, ErrorMessage = "Giá phải lớn hơn 0")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price       { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ảnh hiện tại")]
        public string? ExistingImage { get; set; }

        [Display(Name = "Tải ảnh lên")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool Active { get; set; } = true;
    }
}
