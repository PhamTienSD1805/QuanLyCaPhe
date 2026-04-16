using System.Data;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.Models
{
    public class BillDetail
    {
        public int     Id       { get; set; }
        public int     BillId   { get; set; }
        public int     DrinkId  { get; set; }
        public int     Quantity { get; set; }
        public decimal Price    { get; set; }

        private static BillDetail FromRow(DataRow row) => new()
        {
            Id       = Convert.ToInt32(row["id"]),
            BillId   = Convert.ToInt32(row["bill_id"]),
            DrinkId  = Convert.ToInt32(row["drink_id"]),
            Quantity = Convert.ToInt32(row["quantity"]),
            Price    = Convert.ToDecimal(row["price"])
        };

        public static List<BillDetail> GetAll()
        {
            const string sql = "SELECT id, bill_id, drink_id, quantity, price FROM bill_details";
            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(FromRow).ToList();
        }

        public static BillDetail? GetById(int id)
        {
            const string sql = "SELECT id, bill_id, drink_id, quantity, price FROM bill_details WHERE id = @1";
            var dt = DBUtil.QueryDataTable(sql, id);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        public static List<BillDetail> GetByBill(int billId)
        {
            const string sql = "SELECT id, bill_id, drink_id, quantity, price FROM bill_details WHERE bill_id = @1";
            return DBUtil.QueryDataTable(sql, billId).AsEnumerable().Select(FromRow).ToList();
        }

        public int Insert()
        {
            const string sql = @"INSERT INTO bill_details (bill_id, drink_id, quantity, price)
                                 VALUES (@1, @2, @3, @4);
                                 SELECT SCOPE_IDENTITY();";
            var result = DBUtil.ExecuteScalar(sql, BillId, DrinkId, Quantity, Price);
            return Convert.ToInt32(result);
        }

        public bool Update()
        {
            const string sql = @"UPDATE bill_details
                                 SET bill_id = @1, drink_id = @2, quantity = @3, price = @4
                                 WHERE id = @5";
            return DBUtil.ExecuteNonQuery(sql, BillId, DrinkId, Quantity, Price, Id) > 0;
        }

        public static bool Delete(int id)
        {
            const string sql = "DELETE FROM bill_details WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
