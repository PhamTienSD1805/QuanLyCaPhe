using System.Data;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.Models
{
    /// <summary>
    /// Bill status values
    /// </summary>
    public enum BillStatus
    {
        Pending   = 0,
        Done      = 1,
        Cancelled = 2
    }

    public class Bill
    {
        public int        Id        { get; set; }
        public int        UserId    { get; set; }
        public string     Code      { get; set; } = string.Empty;
        public DateTime   CreatedAt { get; set; } = DateTime.Now;
        public decimal    Total     { get; set; }
        public BillStatus Status    { get; set; } = BillStatus.Pending;

        private static Bill FromRow(DataRow row) => new()
        {
            Id        = Convert.ToInt32(row["id"]),
            UserId    = Convert.ToInt32(row["user_id"]),
            Code      = row["code"].ToString()!,
            CreatedAt = Convert.ToDateTime(row["created_at"]),
            Total     = Convert.ToDecimal(row["total"]),
            Status    = (BillStatus)Convert.ToInt32(row["status"])
        };

        public static List<Bill> GetAll()
        {
            const string sql = "SELECT id, user_id, code, created_at, total, status FROM bills ORDER BY created_at DESC";
            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(FromRow).ToList();
        }

        public static Bill? GetById(int id)
        {
            const string sql = "SELECT id, user_id, code, created_at, total, status FROM bills WHERE id = @1";
            var dt = DBUtil.QueryDataTable(sql, id);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        public static List<Bill> GetByUser(int userId)
        {
            const string sql = "SELECT id, user_id, code, created_at, total, status FROM bills WHERE user_id = @1 ORDER BY created_at DESC";
            return DBUtil.QueryDataTable(sql, userId).AsEnumerable().Select(FromRow).ToList();
        }

        public int Insert()
        {
            const string sql = @"INSERT INTO bills (user_id, code, created_at, total, status)
                                 VALUES (@1, @2, @3, @4, @5);
                                 SELECT SCOPE_IDENTITY();";
            var result = DBUtil.ExecuteScalar(sql, UserId, Code, CreatedAt, Total, (int)Status);
            return Convert.ToInt32(result);
        }

        public bool Update()
        {
            const string sql = @"UPDATE bills
                                 SET user_id = @1, code = @2, created_at = @3, total = @4, status = @5
                                 WHERE id = @6";
            return DBUtil.ExecuteNonQuery(sql, UserId, Code, CreatedAt, Total, (int)Status, Id) > 0;
        }

        public static bool Delete(int id)
        {
            const string sql = "DELETE FROM bills WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
