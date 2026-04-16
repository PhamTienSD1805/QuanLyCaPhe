using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace QuanLyQuanCaPhe.Utils
{
    /// <summary>
    /// Static utility class that centralises all ADO.NET database access.
    /// Connection string is read from appsettings.json: ConnectionStrings:DefaultConnection
    /// </summary>
    public static class DBUtil
    {
        // ----------------------------------------------------------------
        // Read connection string once at class initialisation
        // ----------------------------------------------------------------
        private static readonly string _connectionString;

        static DBUtil()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

            IConfiguration config = builder.Build();
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------

        /// <summary>
        /// Maps a positional parameter list to @1, @2, @3 … SqlParameters.
        /// Strings are explicitly typed as NVARCHAR(MAX) to support Unicode.
        /// </summary>
        private static SqlParameter[] BuildParams(object?[]? paramValues)
        {
            if (paramValues == null || paramValues.Length == 0)
                return Array.Empty<SqlParameter>();

            var list = new List<SqlParameter>();
            for (int i = 0; i < paramValues.Length; i++)
            {
                string name = $"@{i + 1}";
                object value = paramValues[i] ?? DBNull.Value;

                if (value is string s)
                {
                    var p = new SqlParameter(name, SqlDbType.NVarChar, -1) { Value = s };
                    list.Add(p);
                }
                else
                {
                    list.Add(new SqlParameter(name, value));
                }
            }
            return list.ToArray();
        }

        // ----------------------------------------------------------------
        // Public API
        // ----------------------------------------------------------------

        /// <summary>
        /// Executes a SELECT query and returns all rows as a DataTable.
        /// SQL placeholders must be written as @1, @2, @3 …
        /// </summary>
        /// <param name="sql">SQL query with @1…@N placeholders</param>
        /// <param name="paramValues">Parameter values in order</param>
        public static DataTable QueryDataTable(string sql, params object?[]? paramValues)
        {
            var dt = new DataTable();

            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(sql, conn);
                cmd.Parameters.AddRange(BuildParams(paramValues));

                conn.Open();
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DBUtil.QueryDataTable] ERROR: {ex.Message}");
                throw;
            }

            return dt;
        }

        /// <summary>
        /// Executes a non-query (INSERT / UPDATE / DELETE) and returns the
        /// number of rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string sql, params object?[]? paramValues)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(sql, conn);
                cmd.Parameters.AddRange(BuildParams(paramValues));

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DBUtil.ExecuteNonQuery] ERROR: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes a query and returns the value of the first column of the
        /// first row (e.g. SELECT SCOPE_IDENTITY() after an INSERT).
        /// Returns null if the result set is empty.
        /// </summary>
        public static object? ExecuteScalar(string sql, params object?[]? paramValues)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                using var cmd  = new SqlCommand(sql, conn);
                cmd.Parameters.AddRange(BuildParams(paramValues));

                conn.Open();
                object? result = cmd.ExecuteScalar();
                return (result == DBNull.Value) ? null : result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[DBUtil.ExecuteScalar] ERROR: {ex.Message}");
                throw;
            }
        }
    }
}
