using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;

namespace QuanLyQuanCaPhe.Controllers
{
    public class CategoriesController : Controller
    {
        // ----------------------------------------------------------------
        // GET /Categories
        // ----------------------------------------------------------------
        [Authorize(Policy = "EmployeeOrManager")]
        public IActionResult Index()
        {
            var categories = CategoryDAL.GetAll();
            return View(categories);
        }

        // ----------------------------------------------------------------
        // GET /Categories/Details/5
        // ----------------------------------------------------------------
        [Authorize(Policy = "EmployeeOrManager")]
        public IActionResult Details(int id)
        {
            var category = CategoryDAL.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // ----------------------------------------------------------------
        // GET /Categories/Create
        // ----------------------------------------------------------------
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST /Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            category.Insert();
            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // GET /Categories/Edit/5
        // ----------------------------------------------------------------
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Edit(int id)
        {
            var category = CategoryDAL.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST /Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Edit(int id, Category category)
        {
            if (id != category.Id) return BadRequest();
            if (!ModelState.IsValid) return View(category);

            category.Update();
            TempData["Success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // GET /Categories/Delete/5
        // ----------------------------------------------------------------
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Delete(int id)
        {
            var category = CategoryDAL.GetById(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST /Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult DeleteConfirmed(int id)
        {
            CategoryDAL.Delete(id);
            TempData["Success"] = "Xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
