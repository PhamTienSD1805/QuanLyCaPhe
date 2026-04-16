using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;
using System.Diagnostics;

namespace QuanLyQuanCaPhe.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
                return RedirectToAction("Login", "Login");

            var (totalBills, totalRevenue) = BillDAL.GetStats();
            var vm = new DashboardViewModel
            {
                TotalBills   = totalBills,
                TotalRevenue = totalRevenue,
                TotalDrinks  = DrinkDAL.GetActive().Count,
                TotalUsers   = UserDAL.GetAll().Count,
                RecentBills  = BillDAL.GetAll().Take(5).ToList()
            };
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
