using System.Data;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.Models
{
    public class Drink
    {
        public int     Id          { get; set; }
        public int     CategoryId  { get; set; }
        public string  Name        { get; set; } = string.Empty;
        public decimal Price       { get; set; }
        public string? Image       { get; set; }
        public string? Description { get; set; }
        public bool    Active      { get; set; } = true;

        private static Drink FromRow(DataRow row) => new()
        {
            Id          = Convert.ToInt32(row["id"]),
            CategoryId  = Convert.ToInt32(row["category_id"]),
            Name        = row["name"].ToString()!,
            Price       = Convert.ToDecimal(row["price"]),
            Image       = row["image"]       == DBNull.Value ? null : row["image"].ToString(),
            Description = row["description"] == DBNull.Value ? null : row["description"].ToString(),
            Active      = Convert.ToBoolean(row["active"])
        };

        public static List<Drink> GetAll()
        {
            const string sql = "SELECT id, category_id, name, price, image, description, active FROM drinks";
            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(FromRow).ToList();
        }

        public static Drink? GetById(int id)
        {
            const string sql = "SELECT id, category_id, name, price, image, description, active FROM drinks WHERE id = @1";
            var dt = DBUtil.QueryDataTable(sql, id);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        public static List<Drink> GetByCategory(int categoryId)
        {
            const string sql = "SELECT id, category_id, name, price, image, description, active FROM drinks WHERE category_id = @1";
            return DBUtil.QueryDataTable(sql, categoryId).AsEnumerable().Select(FromRow).ToList();
        }

        public int Insert()
        {
            const string sql = @"INSERT INTO drinks (category_id, name, price, image, description, active)
                                 VALUES (@1, @2, @3, @4, @5, @6);
                                 SELECT SCOPE_IDENTITY();";
            var result = DBUtil.ExecuteScalar(sql, CategoryId, Name, Price, Image, Description, Active);
            return Convert.ToInt32(result);
        }

        public bool Update()
        {
            const string sql = @"UPDATE drinks
                                 SET category_id = @1, name = @2, price = @3,
                                     image = @4, description = @5, active = @6
                                 WHERE id = @7";
            return DBUtil.ExecuteNonQuery(sql, CategoryId, Name, Price, Image, Description, Active, Id) > 0;
        }

        public static bool Delete(int id)
        {
            const string sql = "DELETE FROM drinks WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
