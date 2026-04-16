using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;

namespace QuanLyQuanCaPhe.Controllers
{
    public class DrinksController : Controller
    {
        private const string UploadFolder = "image";

        // ----------------------------------------------------------------
        // GET /Drinks  — list + search
        // ----------------------------------------------------------------
        [Authorize(Policy = "EmployeeOrManager")]
        public IActionResult Index(string? name, int? categoryId)
        {
            var drinks     = string.IsNullOrWhiteSpace(name) && !categoryId.HasValue
                             ? DrinkDAL.GetAll()
                             : DrinkDAL.Search(name, categoryId);

            ViewBag.Categories  = new SelectList(CategoryDAL.GetAll(), "Id", "Name", categoryId);
            ViewBag.SearchName  = name;
            ViewBag.CategoryId  = categoryId;
            return View(drinks);
        }

        // ----------------------------------------------------------------
        // GET /Drinks/Details/5
        // ----------------------------------------------------------------
        [Authorize(Policy = "EmployeeOrManager")]
        public IActionResult Details(int id)
        {
            var drink = DrinkDAL.GetById(id);
            if (drink == null) return NotFound();

            ViewBag.CategoryName = CategoryDAL.GetById(drink.CategoryId)?.Name ?? "—";
            return View(drink);
        }

        // ----------------------------------------------------------------
        // GET /Drinks/Create
        // ----------------------------------------------------------------
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(CategoryDAL.GetAll(), "Id", "Name");
            return View(new DrinkViewModel { Active = true });
        }

        // POST /Drinks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Create(DrinkViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(CategoryDAL.GetAll(), "Id", "Name", vm.CategoryId);
                return View(vm);
            }

            string? imagePath = await SaveImageAsync(vm.ImageFile);

            var drink = new Drink
            {
                CategoryId  = vm.CategoryId,
                Name        = vm.Name,
                Price       = vm.Price,
                Description = vm.Description,
                Image       = imagePath,
                Active      = vm.Active
            };
            drink.Insert();

            TempData["Success"] = "Thêm đồ uống thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // GET /Drinks/Edit/5
        // ----------------------------------------------------------------
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Edit(int id)
        {
            var drink = DrinkDAL.GetById(id);
            if (drink == null) return NotFound();

            ViewBag.Categories = new SelectList(CategoryDAL.GetAll(), "Id", "Name", drink.CategoryId);
            return View(new DrinkViewModel
            {
                Id            = drink.Id,
                CategoryId    = drink.CategoryId,
                Name          = drink.Name,
                Price         = drink.Price,
                Description   = drink.Description,
                ExistingImage = drink.Image,
                Active        = drink.Active
            });
        }

        // POST /Drinks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public async Task<IActionResult> Edit(int id, DrinkViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(CategoryDAL.GetAll(), "Id", "Name", vm.CategoryId);
                return View(vm);
            }

            string? imagePath = vm.ImageFile != null
                                ? await SaveImageAsync(vm.ImageFile)
                                : vm.ExistingImage;   // giữ ảnh cũ

            var drink = new Drink
            {
                Id          = vm.Id,
                CategoryId  = vm.CategoryId,
                Name        = vm.Name,
                Price       = vm.Price,
                Description = vm.Description,
                Image       = imagePath,
                Active      = vm.Active
            };
            drink.Update();

            TempData["Success"] = "Cập nhật đồ uống thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // GET /Drinks/Delete/5
        // ----------------------------------------------------------------
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Delete(int id)
        {
            var drink = DrinkDAL.GetById(id);
            if (drink == null) return NotFound();
            return View(drink);
        }

        // POST /Drinks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult DeleteConfirmed(int id)
        {
            DrinkDAL.Deactivate(id);     // soft delete
            TempData["Success"] = "Đã ngưng kích hoạt đồ uống!";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------------------
        // Helper — save uploaded image to wwwroot/uploads/drinks/
        // ----------------------------------------------------------------
        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string ext    = Path.GetExtension(file.FileName);
            string name   = Guid.NewGuid().ToString("N") + ext;
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", UploadFolder);
            Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, name);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{UploadFolder}/{name}";
        }
    }
}
