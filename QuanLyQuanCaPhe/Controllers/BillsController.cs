using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;
using System.Security.Claims;
using System.Text.Json;

namespace QuanLyQuanCaPhe.Controllers
{
    [Authorize(Policy = "EmployeeOrManager")]
    public class BillsController : Controller
    {
        private const string CartKey = "pos_cart";

        // ----------------------------------------------------------------
        // GET /Bills  — Employee: bills của mình; Manager: tất cả + filter
        // ----------------------------------------------------------------
        public IActionResult Index(string? status = null)
        {
            List<BillRowViewModel> bills;

            if (User.IsInRole("Manager"))
            {
                // Manager: lấy tất cả kèm tên nhân viên
                bills = BillDAL.GetAllWithEmployee();

                // Lọc theo trạng thái nếu có
                if (!string.IsNullOrEmpty(status) && int.TryParse(status, out int statusInt))
                    bills = bills.Where(b => (int)b.Status == statusInt).ToList();
            }
            else
            {
                // Employee: chỉ thấy bill của mình, wrap sang BillRowViewModel
                int userId    = GetCurrentUserId();
                var ownBills  = BillDAL.GetByUser(userId);
                var empName   = User.Identity?.Name ?? "";
                bills = ownBills.Select(b => new BillRowViewModel
                {
                    Id           = b.Id,
                    Code         = b.Code,
                    CreatedAt    = b.CreatedAt,
                    Total        = b.Total,
                    Status       = b.Status,
                    EmployeeName = empName
                }).ToList();
            }

            ViewBag.FilterStatus = status ?? "";
            return View(bills);
        }

        // ----------------------------------------------------------------
        // GET /Bills/Details/5
        // ----------------------------------------------------------------
        public IActionResult Details(int id)
        {
            var bill = BillDAL.GetById(id);
            if (bill == null) return NotFound();

            // Employee chỉ xem bill của mình
            if (!User.IsInRole("Manager") && bill.UserId != GetCurrentUserId())
                return Forbid();

            var details = BillDetailDAL.GetByBill(id);
            var drinks  = DrinkDAL.GetAll().ToDictionary(d => d.Id);

            ViewBag.Bill         = bill;
            ViewBag.Drinks       = drinks;
            ViewBag.EmployeeName = BillDAL.GetEmployeeName(id);
            return View(details);
        }

        // ================================================================
        // POS — Create Bill
        // ================================================================

        // GET /Bills/Create
        public IActionResult Create(string? name, int? categoryId)
        {
            var cart = GetCart();

            var vm = new PosViewModel
            {
                Drinks     = DrinkDAL.Search(name, categoryId),
                Categories = CategoryDAL.GetAll(),
                Cart       = cart,
                SearchName = name,
                CategoryId = categoryId
            };
            return View(vm);
        }

        // POST /Bills/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int drinkId, int qty = 1)
        {
            var drink = DrinkDAL.GetById(drinkId);
            if (drink == null) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.DrinkId == drinkId);
            if (item != null)
            {
                item.Quantity += qty;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    DrinkId   = drinkId,
                    DrinkName = drink.Name,
                    Quantity  = qty,
                    UnitPrice = drink.Price
                });
            }
            SaveCart(cart);
            return RedirectToAction(nameof(Create));
        }

        // POST /Bills/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int drinkId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.DrinkId == drinkId);
            SaveCart(cart);
            return RedirectToAction(nameof(Create));
        }

        // POST /Bills/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống! Vui lòng thêm ít nhất 1 đồ uống.";
                return RedirectToAction(nameof(Create));
            }

            int userId = GetCurrentUserId();
            var bill   = new Bill
            {
                UserId    = userId,
                Code      = BillDAL.GenerateCode(),
                CreatedAt = DateTime.Now,
                Status    = BillStatus.Done,
                Total     = cart.Sum(i => i.LineTotal)
            };
            int billId = BillDAL.Create(bill);

            foreach (var item in cart)
            {
                var detail = new BillDetail
                {
                    BillId   = billId,
                    DrinkId  = item.DrinkId,
                    Quantity = item.Quantity,
                    Price    = item.UnitPrice
                };
                BillDetailDAL.Create(detail);
            }

            // Xóa giỏ hàng
            HttpContext.Session.Remove(CartKey);

            TempData["Success"] = $"Thanh toán thành công! Mã hóa đơn: {bill.Code}";
            return RedirectToAction(nameof(Details), new { id = billId });
        }

        // ----------------------------------------------------------------
        // POST /Bills/Cancel/5  (Manager only)
        // ----------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOnly")]
        public IActionResult Cancel(int id)
        {
            var bill = BillDAL.GetById(id);
            if (bill == null) return NotFound();

            BillDAL.UpdateStatus(id, BillStatus.Cancelled);
            TempData["Success"] = "Đã hủy hóa đơn!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ================================================================
        // Private helpers
        // ================================================================
        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        private List<CartItemViewModel> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json)) return [];
            return JsonSerializer.Deserialize<List<CartItemViewModel>>(json) ?? [];
        }

        private void SaveCart(List<CartItemViewModel> cart) =>
            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
    }
}
