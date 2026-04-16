using System.Data;
using QuanLyQuanCaPhe.Utils;

namespace QuanLyQuanCaPhe.Models
{
    public class User
    {
        public int     Id       { get; set; }
        public string  Email    { get; set; } = string.Empty;
        public string  Password { get; set; } = string.Empty;
        public string  FullName { get; set; } = string.Empty;
        public string? Phone    { get; set; }
        /// <summary>true = Manager, false = Employee</summary>
        public bool    Role     { get; set; }
        public bool    Active   { get; set; } = true;

        private static User FromRow(DataRow row) => new()
        {
            Id       = Convert.ToInt32(row["id"]),
            Email    = row["email"].ToString()!,
            Password = row["password"].ToString()!,
            FullName = row["full_name"].ToString()!,
            Phone    = row["phone"] == DBNull.Value ? null : row["phone"].ToString(),
            Role     = Convert.ToBoolean(row["role"]),
            Active   = Convert.ToBoolean(row["active"])
        };

        public static List<User> GetAll()
        {
            const string sql = "SELECT id, email, password, full_name, phone, role, active FROM users";
            return DBUtil.QueryDataTable(sql).AsEnumerable().Select(FromRow).ToList();
        }

        public static User? GetById(int id)
        {
            const string sql = "SELECT id, email, password, full_name, phone, role, active FROM users WHERE id = @1";
            var dt = DBUtil.QueryDataTable(sql, id);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        public static User? GetByEmail(string email)
        {
            const string sql = "SELECT id, email, password, full_name, phone, role, active FROM users WHERE email = @1";
            var dt = DBUtil.QueryDataTable(sql, email);
            return dt.Rows.Count > 0 ? FromRow(dt.Rows[0]) : null;
        }

        public int Insert()
        {
            const string sql = @"INSERT INTO users (email, password, full_name, phone, role, active)
                                 VALUES (@1, @2, @3, @4, @5, @6);
                                 SELECT SCOPE_IDENTITY();";
            var result = DBUtil.ExecuteScalar(sql, Email, Password, FullName, Phone, Role, Active);
            return Convert.ToInt32(result);
        }

        public bool Update()
        {
            const string sql = @"UPDATE users
                                 SET email = @1, password = @2, full_name = @3,
                                     phone = @4, role = @5, active = @6
                                 WHERE id = @7";
            return DBUtil.ExecuteNonQuery(sql, Email, Password, FullName, Phone, Role, Active, Id) > 0;
        }

        public static bool Delete(int id)
        {
            const string sql = "DELETE FROM users WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
