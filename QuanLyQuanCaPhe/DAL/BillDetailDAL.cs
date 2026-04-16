using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.DAL
{
    public class BillDetailDAL
    {
        public static List<BillDetail> GetAll() => BillDetail.GetAll();

        public static BillDetail? GetById(int id) => BillDetail.GetById(id);

        public static List<BillDetail> GetByBill(int billId) => BillDetail.GetByBill(billId);

        public static int Create(BillDetail detail) => detail.Insert();

        public static bool Update(BillDetail detail) => detail.Update();

        public static bool Delete(int id) => BillDetail.Delete(id);

        /// <summary>Removes all detail lines for a given bill (e.g. when cancelling).</summary>
        public static bool DeleteByBill(int billId)
        {
            const string sql = "DELETE FROM bill_details WHERE bill_id = @1";
            return DBUtil.ExecuteNonQuery(sql, billId) > 0;
        }
    }
}
