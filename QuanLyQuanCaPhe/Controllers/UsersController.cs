using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;

namespace QuanLyQuanCaPhe.Controllers
{
    [Authorize(Policy = "ManagerOnly")]
    public class UsersController : Controller
    {
        // ----------------------------------------------------------------
        // GET /Users
        // ----------------------------------------------------------------
        public IActionResult Index()
        {
            var users = UserDAL.GetAll();
            return View(users);
        }

        // ----------------------------------------------------------------
        // GET /Users/Create
        // ----------------------------------------------------------------
        public IActionResult Create()
        {
            ViewBag.Roles = RoleList();
            return View(new UserViewModel { Active = true });
        }

        // POST /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError(nameof(vm.Password), "Mật khẩu là bắt buộc khi tạo mới");

            if (UserDAL.EmailExists(vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "Email này đã tồn tại trong hệ thống");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = RoleList(vm.Role);
                return View(vm);
            }

            var user = new User
            {
                Email    = vm.Email,
                Password = vm.Password!,
                FullName = vm.FullName,
                Phone    = vm.Phone,
                Role     = vm.Role,
                Active   = vm.Active
            };
            UserDAL.Create(user);

            TempData["Success"] = "Thêm nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // GET /Users/Edit/5
        // ----------------------------------------------------------------
        public IActionResult Edit(int id)
        {
            var user = UserDAL.GetById(id);
            if (user == null) return NotFound();

            ViewBag.Roles = RoleList(user.Role);
            return View(new UserViewModel
            {
                Id       = user.Id,
                Email    = user.Email,
                FullName = user.FullName,
                Phone    = user.Phone,
                Role     = user.Role,
                Active   = user.Active
                // Password để trống → giữ nguyên
            });
        }

        // POST /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UserViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            // Kiểm tra email trùng (trừ chính user đang sửa)
            var existing = UserDAL.GetByEmail(vm.Email);
            if (existing != null && existing.Id != id)
                ModelState.AddModelError(nameof(vm.Email), "Email này đã được sử dụng");

            // Password không bắt buộc khi edit
            if (!string.IsNullOrWhiteSpace(vm.Password) && vm.Password.Length < 4)
            {
                // validation được xử lý bởi [MinLength] attribute trên ViewModel
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = RoleList(vm.Role);
                return View(vm);
            }

            var current = UserDAL.GetById(id)!;
            current.Email    = vm.Email;
            current.FullName = vm.FullName;
            current.Phone    = vm.Phone;
            current.Role     = vm.Role;
            current.Active   = vm.Active;
            if (!string.IsNullOrWhiteSpace(vm.Password))
                current.Password = vm.Password;   // chỉ cập nhật nếu nhập mới

            UserDAL.Update(current);

            TempData["Success"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // GET /Users/Delete/5
        // ----------------------------------------------------------------
        public IActionResult Delete(int id)
        {
            var user = UserDAL.GetById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = UserDAL.GetById(id);
            if (user == null) return NotFound();

            // Bảo vệ: không cho khóa tài khoản Quản lý
            if (user.Role)
            {
                TempData["Error"] = "Không thể khóa tài khoản Quản lý.";
                return RedirectToAction(nameof(Index));
            }

            UserDAL.Deactivate(id);   // soft delete
            TempData["Success"] = "Đã vô hiệu hóa tài khoản!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // Helper
        // ----------------------------------------------------------------
        private static SelectList RoleList(bool selected = false) =>
            new(new[]
            {
                new { Value = "False", Text = "Nhân viên" },
                new { Value = "True",  Text = "Quản lý"   }
            }, "Value", "Text", selected.ToString());
    }
}
