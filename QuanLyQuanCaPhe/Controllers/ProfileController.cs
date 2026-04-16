using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;
using System.Security.Claims;

namespace QuanLyQuanCaPhe.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        // ----------------------------------------------------------------
        // GET /Profile — Xem & chỉnh sửa hồ sơ
        // ----------------------------------------------------------------
        [HttpGet]
        public IActionResult Index()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");

            if (TempData["Success"] is string ok) ViewBag.SuccessMessage = ok;
            if (TempData["Error"]   is string er) ViewBag.ErrorMessage   = er;

            return View(MapToVm(user));
        }

        // POST /Profile — Lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProfileViewModel vm)
        {
            // Nếu nhập mật khẩu mới → bắt buộc nhập mật khẩu hiện tại
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(vm.CurrentPassword))
                    ModelState.AddModelError(nameof(vm.CurrentPassword),
                        "Vui lòng nhập mật khẩu hiện tại để thay đổi mật khẩu.");
            }

            // Loại bỏ lỗi của các trường password nếu không muốn đổi
            if (string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                ModelState.Remove(nameof(vm.NewPassword));
                ModelState.Remove(nameof(vm.ConfirmPassword));
                ModelState.Remove(nameof(vm.CurrentPassword));
            }

            if (!ModelState.IsValid)
                return View(vm);

            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");

            // Kiểm tra mật khẩu hiện tại
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                if (user.Password != vm.CurrentPassword)
                {
                    ModelState.AddModelError(nameof(vm.CurrentPassword),
                        "Mật khẩu hiện tại không đúng.");
                    return View(vm);
                }
                user.Password = vm.NewPassword;
            }

            // Cập nhật thông tin cá nhân
            user.FullName = vm.FullName;
            user.Phone    = vm.Phone;
            UserDAL.Update(user);

            TempData["Success"] = "Hồ sơ đã được cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // Private helpers
        // ----------------------------------------------------------------
        private User? GetCurrentUser()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out int id)) return null;
            return UserDAL.GetById(id);
        }

        private static ProfileViewModel MapToVm(User u) => new()
        {
            Id       = u.Id,
            Email    = u.Email,
            FullName = u.FullName,
            Phone    = u.Phone,
            Active   = u.Active
        };
    }
}
