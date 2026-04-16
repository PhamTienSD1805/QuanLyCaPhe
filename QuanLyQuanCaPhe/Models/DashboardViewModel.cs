namespace QuanLyQuanCaPhe.Models
{
    public class DashboardViewModel
    {
        public int         TotalBills   { get; set; }
        public decimal     TotalRevenue { get; set; }
        public int         TotalDrinks  { get; set; }
        public int         TotalUsers   { get; set; }
        public List<Bill>  RecentBills  { get; set; } = [];
    }
}
