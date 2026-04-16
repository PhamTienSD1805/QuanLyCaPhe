using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;
using System.Data;

namespace QuanLyQuanCaPhe.DAL
{
    /// <summary>
    /// Data Access Layer for Category entity.
    /// All methods delegate to DBUtil for parameterised SQL execution.
    /// </summary>
    public class CategoryDAL
    {
        // ----------------------------------------------------------------
        // READ
        // ----------------------------------------------------------------

        /// <summary>Returns all active categories.</summary>
        public static List<Category> GetActive()
        {
            const string sql = "SELECT id, name, active FROM categories WHERE active = 1 ORDER BY name";
            return DBUtil.QueryDataTable(sql)
                         .AsEnumerable()
                         .Select(row => new Category
                         {
                             Id     = Convert.ToInt32(row["id"]),
                             Name   = row["name"].ToString()!,
                             Active = Convert.ToBoolean(row["active"])
                         })
                         .ToList();
        }

        /// <summary>Returns all categories (active and inactive).</summary>
        public static List<Category> GetAll() => Category.GetAll();

        /// <summary>Returns a category by id, or null.</summary>
        public static Category? GetById(int id) => Category.GetById(id);

        // ----------------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------------

        /// <summary>Inserts a new category and returns its generated id.</summary>
        public static int Create(string name, bool active = true)
        {
            var category = new Category { Name = name, Active = active };
            return category.Insert();
        }

        // ----------------------------------------------------------------
        // UPDATE
        // ----------------------------------------------------------------

        /// <summary>Updates an existing category. Returns true on success.</summary>
        public static bool Update(int id, string name, bool active)
        {
            var category = new Category { Id = id, Name = name, Active = active };
            return category.Update();
        }

        // ----------------------------------------------------------------
        // DELETE
        // ----------------------------------------------------------------

        /// <summary>
        /// Hard-deletes a category. Consider soft-delete (active = 0) in production.
        /// </summary>
        public static bool Delete(int id) => Category.Delete(id);

        /// <summary>Soft-deletes by setting active = 0.</summary>
        public static bool Deactivate(int id)
        {
            const string sql = "UPDATE categories SET active = 0 WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
