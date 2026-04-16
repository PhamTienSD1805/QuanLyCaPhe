using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;
using System.Data;

namespace QuanLyQuanCaPhe.DAL
{
    public class BillDAL
    {
        public static List<Bill> GetAll() => Bill.GetAll();

        public static Bill? GetById(int id) => Bill.GetById(id);

        public static List<Bill> GetByUser(int userId) => Bill.GetByUser(userId);

        /// <summary>Generates a unique bill code based on current timestamp.</summary>
        public static string GenerateCode()
            => "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");

        public static int Create(Bill bill) => bill.Insert();

        public static bool UpdateStatus(int id, BillStatus status)
        {
            const string sql = "UPDATE bills SET status = @1 WHERE id = @2";
            return DBUtil.ExecuteNonQuery(sql, (int)status, id) > 0;
        }

        public static bool Update(Bill bill) => bill.Update();

        public static bool Delete(int id) => Bill.Delete(id);

        /// <summary>Recalculates the bill total from its detail lines and saves.</summary>
        public static bool RecalculateTotal(int billId)
        {
            const string sql = @"UPDATE bills
                                 SET total = (
                                     SELECT ISNULL(SUM(quantity * price), 0)
                                     FROM bill_details
                                     WHERE bill_id = @1
                                 )
                                 WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, billId) > 0;
        }

        /// <summary>Returns dashboard statistics: (totalBills, totalRevenue).</summary>
        public static (int TotalBills, decimal TotalRevenue) GetStats()
        {
            const string sql = @"SELECT COUNT(*) AS total_bills,
                                        ISNULL(SUM(total), 0) AS total_revenue
                                 FROM bills
                                 WHERE status <> 2";
            var dt = DBUtil.QueryDataTable(sql);
            if (dt.Rows.Count == 0) return (0, 0);
            var row = dt.Rows[0];
            return (Convert.ToInt32(row["total_bills"]),
                    Convert.ToDecimal(row["total_revenue"]));
        }

        // ================================================================
        // LAB 6 — Statistics queries
        // ================================================================

        /// <summary>
        /// Trả về danh sách hóa đơn kèm tên nhân viên tạo đơn.
        /// Dùng cho Bills/Index (Manager view).
        /// </summary>
        public static List<BillRowViewModel> GetAllWithEmployee()
        {
            const string sql = @"
                SELECT b.id, b.code, b.created_at, b.total, b.status,
                       u.full_name AS employee_name,
                       u.email    AS employee_email
                FROM bills b
                INNER JOIN users u ON u.id = b.user_id
                ORDER BY b.created_at DESC";

            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(row => new BillRowViewModel
            {
                Id            = Convert.ToInt32(row["id"]),
                Code          = row["code"].ToString()!,
                CreatedAt     = Convert.ToDateTime(row["created_at"]),
                Total         = Convert.ToDecimal(row["total"]),
                Status        = (BillStatus)Convert.ToInt32(row["status"]),
                EmployeeName  = row["employee_name"].ToString()!,
                EmployeeEmail = row["employee_email"].ToString()!
            }).ToList();
        }

        /// <summary>
        /// Trả về tên nhân viên tạo hóa đơn theo bill_id.
        /// Dùng cho Bills/Details.
        /// </summary>
        public static string GetEmployeeName(int billId)
        {
            const string sql = @"
                SELECT u.full_name
                FROM bills b
                INNER JOIN users u ON u.id = b.user_id
                WHERE b.id = @1";
            var result = DBUtil.ExecuteScalar(sql, billId);
            return result?.ToString() ?? "—";
        }

        /// <summary>
        /// Top 5 đồ uống bán chạy nhất (theo tổng số lượng), chỉ tính hóa đơn Done.
        /// </summary>
        public static List<TopDrinkItem> GetTop5BestSelling()
        {
            const string sql = @"
                SELECT TOP 5
                    d.name                          AS drink_name,
                    SUM(bd.quantity)                AS total_sold,
                    SUM(bd.quantity * bd.price)     AS revenue
                FROM bill_details bd
                INNER JOIN drinks d  ON d.id  = bd.drink_id
                INNER JOIN bills  b  ON b.id  = bd.bill_id
                WHERE b.status = 1   -- Done only
                GROUP BY d.id, d.name
                ORDER BY total_sold DESC";

            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(row => new TopDrinkItem
            {
                DrinkName = row["drink_name"].ToString()!,
                TotalSold = Convert.ToInt32(row["total_sold"]),
                Revenue   = Convert.ToDecimal(row["revenue"])
            }).ToList();
        }

        /// <summary>
        /// Doanh thu theo từng NGÀY trong một tháng cụ thể.
        /// </summary>
        public static List<RevenuePoint> GetRevenueByDay(int year, int month)
        {
            const string sql = @"
                SELECT
                    DAY(created_at)              AS lbl,
                    ISNULL(SUM(total), 0)        AS amount,
                    COUNT(*)                     AS orders
                FROM bills
                WHERE status = 1
                  AND YEAR(created_at)  = @1
                  AND MONTH(created_at) = @2
                GROUP BY DAY(created_at)
                ORDER BY DAY(created_at)";

            return DBUtil.QueryDataTable(sql, year, month).AsEnumerable().Select(row => new RevenuePoint
            {
                Label  = $"{Convert.ToInt32(row["lbl"]):D2}/{month:D2}",
                Amount = Convert.ToDecimal(row["amount"]),
                Orders = Convert.ToInt32(row["orders"])
            }).ToList();
        }

        /// <summary>
        /// Doanh thu theo từng THÁNG trong năm.
        /// </summary>
        public static List<RevenuePoint> GetRevenueByMonth(int year)
        {
            const string sql = @"
                SELECT
                    MONTH(created_at)            AS lbl,
                    ISNULL(SUM(total), 0)        AS amount,
                    COUNT(*)                     AS orders
                FROM bills
                WHERE status = 1
                  AND YEAR(created_at) = @1
                GROUP BY MONTH(created_at)
                ORDER BY MONTH(created_at)";

            return DBUtil.QueryDataTable(sql, year).AsEnumerable().Select(row => new RevenuePoint
            {
                Label  = $"Tháng {Convert.ToInt32(row["lbl"])}",
                Amount = Convert.ToDecimal(row["amount"]),
                Orders = Convert.ToInt32(row["orders"])
            }).ToList();
        }

        /// <summary>
        /// Doanh thu theo từng NĂM (5 năm gần nhất).
        /// </summary>
        public static List<RevenuePoint> GetRevenueByYear()
        {
            const string sql = @"
                SELECT TOP 5
                    YEAR(created_at)             AS lbl,
                    ISNULL(SUM(total), 0)        AS amount,
                    COUNT(*)                     AS orders
                FROM bills
                WHERE status = 1
                GROUP BY YEAR(created_at)
                ORDER BY YEAR(created_at)";

            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(row => new RevenuePoint
            {
                Label  = $"Năm {Convert.ToInt32(row["lbl"])}",
                Amount = Convert.ToDecimal(row["amount"]),
                Orders = Convert.ToInt32(row["orders"])
            }).ToList();
        }
    }
}
