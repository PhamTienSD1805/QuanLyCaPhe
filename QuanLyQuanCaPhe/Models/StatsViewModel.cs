namespace QuanLyQuanCaPhe.Models
{
    // ====================================================================
    // Top-selling drink item
    // ====================================================================
    public class TopDrinkItem
    {
        public string DrinkName   { get; set; } = string.Empty;
        public int    TotalSold   { get; set; }   // tổng số lượng bán ra
        public decimal Revenue    { get; set; }   // doanh thu từ đồ uống này
    }

    // ====================================================================
    // Revenue data point (for line/bar chart)
    // ====================================================================
    public class RevenuePoint
    {
        public string  Label   { get; set; } = string.Empty;  // "01/04", "Tháng 4", "2024"
        public decimal Amount  { get; set; }
        public int     Orders  { get; set; }   // số hoá đơn trong kỳ
    }

    // ====================================================================
    // Bill row with employee name (for Bills/Index manager view)
    // ====================================================================
    public class BillRowViewModel
    {
        public int        Id           { get; set; }
        public string     Code         { get; set; } = string.Empty;
        public DateTime   CreatedAt    { get; set; }
        public decimal    Total        { get; set; }
        public BillStatus Status       { get; set; }
        public string     EmployeeName { get; set; } = string.Empty;
        public string     EmployeeEmail{ get; set; } = string.Empty;
    }

    // ====================================================================
    // Statistics page ViewModel
    // ====================================================================
    public class StatsViewModel
    {
        // Filter params
        public string  Mode      { get; set; } = "month"; // "day" | "month" | "year"
        public int     Year      { get; set; } = DateTime.Now.Year;
        public int?    Month     { get; set; } = DateTime.Now.Month;

        // Data for charts
        public List<TopDrinkItem>  TopDrinks   { get; set; } = [];
        public List<RevenuePoint>  RevenueData { get; set; } = [];

        // Summary totals
        public decimal TotalRevenue { get; set; }
        public int     TotalOrders  { get; set; }
    }
}
