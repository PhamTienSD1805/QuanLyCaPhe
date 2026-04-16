using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;
using System.Data;

namespace QuanLyQuanCaPhe.DAL
{
    /// <summary>
    /// Data Access Layer for User entity.
    /// Provides authentication and standard CRUD operations.
    /// </summary>
    public class UserDAL
    {
        // ----------------------------------------------------------------
        // AUTHENTICATION
        // ----------------------------------------------------------------

        /// <summary>
        /// Validates email + password.
        /// Returns the matching User when credentials are correct; null otherwise.
        /// NOTE: In production store a salted hash instead of a plain-text password.
        /// </summary>
        public static User? Authenticate(string email, string password)
        {
            const string sql = @"SELECT id, email, password, full_name, phone, role, active
                                 FROM users
                                 WHERE email = @1 AND password = @2 AND active = 1";
            var dt = DBUtil.QueryDataTable(sql, email, password);
            if (dt.Rows.Count == 0) return null;

            var row = dt.Rows[0];
            return new User
            {
                Id       = Convert.ToInt32(row["id"]),
                Email    = row["email"].ToString()!,
                Password = row["password"].ToString()!,
                FullName = row["full_name"].ToString()!,
                Phone    = row["phone"] == DBNull.Value ? null : row["phone"].ToString(),
                Role     = Convert.ToBoolean(row["role"]),
                Active   = Convert.ToBoolean(row["active"])
            };
        }

        // ----------------------------------------------------------------
        // READ
        // ----------------------------------------------------------------

        public static List<User> GetAll() => User.GetAll();

        public static User? GetById(int id) => User.GetById(id);

        public static User? GetByEmail(string email) => User.GetByEmail(email);

        /// <summary>Returns true when the e-mail is already registered.</summary>
        public static bool EmailExists(string email)
        {
            const string sql = "SELECT COUNT(1) FROM users WHERE email = @1";
            var result = DBUtil.ExecuteScalar(sql, email);
            return Convert.ToInt32(result) > 0;
        }

        // ----------------------------------------------------------------
        // CREATE
        // ----------------------------------------------------------------

        public static int Create(User user) => user.Insert();

        // ----------------------------------------------------------------
        // UPDATE
        // ----------------------------------------------------------------

        public static bool Update(User user) => user.Update();

        // ----------------------------------------------------------------
        // DELETE / DEACTIVATE
        // ----------------------------------------------------------------

        public static bool Delete(int id) => User.Delete(id);

        public static bool Deactivate(int id)
        {
            const string sql = "UPDATE users SET active = 0 WHERE id = @1";
            return DBUtil.ExecuteNonQuery(sql, id) > 0;
        }
    }
}
