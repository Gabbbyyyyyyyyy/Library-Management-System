using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Data.SQLite;

namespace LibraryManagementSystem.Data
{
    public static class DatabaseHelper
    {
        // Get the base project directory (3 levels up from bin/Debug)
        private static readonly string ProjectPath = Path.GetFullPath(
            Path.Combine(Application.StartupPath, @"..\..\..")
        );

        // Point to your Data/library.db inside the project
        private static readonly string DbPath = Path.Combine(ProjectPath, "Data", "library.db");

        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static DataTable Query(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static int Execute(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(ConnectionString))
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                if (parameters != null) cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
