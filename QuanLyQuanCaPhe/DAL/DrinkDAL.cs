using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;
using System.Data;

namespace QuanLyQuanCaPhe.DAL
{
    public class DrinkDAL
    {
        public static List<Drink> GetAll() => Drink.GetAll();

        public static Drink? GetById(int id) => Drink.GetById(id);

        public static List<Drink> GetByCategory(int categoryId) => Drink.GetByCategory(categoryId);

        public static List<Drink> GetActive()
        {
            const string sql = "SELECT id, category_id, name, price, image, description, active FROM drinks WHERE active = 1 ORDER BY name";
            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(row => new Drink
            {
                Id          = Convert.ToInt32(row["id"]),
                CategoryId  = Convert.ToInt32(row["category_id"]),
                Name        = row["name"].ToString()!,
                Price       = Convert.ToDecimal(row["price"]),
                Image       = row["image"]       == DBNull.Value ? null : row["image"].ToString(),
                Description = row["description"] == DBNull.Value ? null : row["description"].ToString(),
                Active      = Convert.ToBoolean(row["active"])
            }).ToList();
        }

        public static int Create(Drink drink) => drink.Insert();

        public static bool Update(Drink drink) => drink.Update();

        public static bool Delete(int id) => Drink.Delete(id);

        public static bool Deactivate(int id)
        {
            const string sql = "UPDATE drinks SET active = 0 WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }

        /// <summary>Tìm kiếm đồ uống theo tên (LIKE) và/hoặc danh mục.</summary>
        public static List<Drink> Search(string? name, int? categoryId)
        {
            var conditions = new List<string> { "active = 1" };
            var parameters = new List<object>();
            int p         = 1;

            if (!string.IsNullOrWhiteSpace(name))
            {
                conditions.Add($"name LIKE @{p++}");
                parameters.Add("%" + name.Trim() + "%");
            }
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                conditions.Add($"category_id = @{p++}");
                parameters.Add(categoryId.Value);
            }

            string sql = $"SELECT id, category_id, name, price, image, description, active FROM drinks WHERE {string.Join(" AND ", conditions)} ORDER BY name";
            return DBUtil.QueryDataTable(sql, parameters.ToArray()).AsEnumerable().Select(row => new Drink
            {
                Id          = Convert.ToInt32(row["id"]),
                CategoryId  = Convert.ToInt32(row["category_id"]),
                Name        = row["name"].ToString()!,
                Price       = Convert.ToDecimal(row["price"]),
                Image       = row["image"]       == DBNull.Value ? null : row["image"].ToString(),
                Description = row["description"] == DBNull.Value ? null : row["description"].ToString(),
                Active      = Convert.ToBoolean(row["active"])
            }).ToList();
        }
    }
}
