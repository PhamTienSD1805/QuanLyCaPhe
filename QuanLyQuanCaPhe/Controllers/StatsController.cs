using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;
using System.Text.Json;

namespace QuanLyQuanCaPhe.Controllers
{
    /// <summary>
    /// LAB 6 — Thống kê doanh thu và đồ uống bán chạy.
    /// Chỉ Manager được truy cập.
    /// </summary>
    [Authorize(Policy = "ManagerOnly")]
    public class StatsController : Controller
    {
        // ----------------------------------------------------------------
        // GET /Stats  — Trang thống kê chính
        // ----------------------------------------------------------------
        public IActionResult Index(string mode = "month", int? year = null, int? month = null)
        {
            int currentYear  = year  ?? DateTime.Now.Year;
            int currentMonth = month ?? DateTime.Now.Month;

            // ── Xây dựng dữ liệu theo mode ──────────────────────────────
            List<RevenuePoint> revenueData = mode switch
            {
                "day"  => BillDAL.GetRevenueByDay(currentYear, currentMonth),
                "year" => BillDAL.GetRevenueByYear(),
                _      => BillDAL.GetRevenueByMonth(currentYear)   // default: "month"
            };

            var vm = new StatsViewModel
            {
                Mode        = mode,
                Year        = currentYear,
                Month       = currentMonth,
                TopDrinks   = BillDAL.GetTop5BestSelling(),
                RevenueData = revenueData,
                TotalRevenue = revenueData.Sum(r => r.Amount),
                TotalOrders  = revenueData.Sum(r => r.Orders)
            };

            // ── Serialize dữ liệu → JSON để Chart.js đọc ────────────────
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Revenue chart data
            ViewBag.RevenueLabels  = JsonSerializer.Serialize(vm.RevenueData.Select(r => r.Label).ToList(),  jsonOptions);
            ViewBag.RevenueAmounts = JsonSerializer.Serialize(vm.RevenueData.Select(r => r.Amount).ToList(), jsonOptions);
            ViewBag.RevenueOrders  = JsonSerializer.Serialize(vm.RevenueData.Select(r => r.Orders).ToList(), jsonOptions);

            // Top5 chart data
            ViewBag.Top5Names    = JsonSerializer.Serialize(vm.TopDrinks.Select(d => d.DrinkName).ToList(), jsonOptions);
            ViewBag.Top5Sold     = JsonSerializer.Serialize(vm.TopDrinks.Select(d => d.TotalSold).ToList(), jsonOptions);
            ViewBag.Top5Revenue  = JsonSerializer.Serialize(vm.TopDrinks.Select(d => d.Revenue).ToList(),   jsonOptions);

            return View(vm);
        }
    }
}
