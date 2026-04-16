using System.Data;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.Models
{
    public class Category
    {
        public int    Id     { get; set; }
        public string Name   { get; set; } = string.Empty;
        public bool   Active { get; set; } = true;

        // ----------------------------------------------------------------
        // Map a DataRow → Category
        // ----------------------------------------------------------------
        private static Category FromRow(DataRow row) => new()
        {
            Id     = Convert.ToInt32(row["id"]),
            Name   = row["name"].ToString()!,
            Active = Convert.ToBoolean(row["active"])
        };

        // ----------------------------------------------------------------
        // CRUD
        // ----------------------------------------------------------------

        /// <summary>Returns all categories.</summary>
        public static List<Category> GetAll()
        {
            const string sql = "SELECT id, name, active FROM categories";
            return DBUtil.QueryDataTable(sql)
                         .AsEnumerable()
                         .Select(FromRow)
                         .ToList();
        }

        /// <summary>Returns a single category by primary key, or null.</summary>
        public static Category? GetById(int id)
        {
            const string sql = "SELECT id, name, active FROM categories WHERE id = @1";
            var dt = DBUtil.QueryDataTable(sql, id);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        /// <summary>Inserts this category and returns the new identity value.</summary>
        public int Insert()
        {
            const string sql = @"INSERT INTO categories (name, active)
                                 VALUES (@1, @2);
                                 SELECT SCOPE_IDENTITY();";
            var result = DBUtil.ExecuteScalar(sql, Name, Active);
            return Convert.ToInt32(result);
        }

        /// <summary>Updates this category (matched by Id).</summary>
        public bool Update()
        {
            const string sql = "UPDATE categories SET name = @1, active = @2 WHERE id = @3";
            return DBUtil.ExecuteNonQuery(sql, Name, Active, Id) > 0;
        }

        /// <summary>Deletes a category by primary key.</summary>
        public static bool Delete(int id)
        {
            const string sql = "DELETE FROM categories WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
