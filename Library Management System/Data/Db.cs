using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using Library_Management_System.Data;

namespace LibraryManagementSystem.Data
{
    public static class Db
    {
        public static string DbPath => Path.Combine(Application.StartupPath, "Data", "library.db");
        public static string ConnectionString => $"Data Source={DbPath};Version=3;";
    

public static void EnsureCreated()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (var con = GetConnection())
            {
                con.Open();

                // Call EnsureCreated() for each table separately
                Members.EnsureCreated(con);
                Books.EnsureCreated(con);
                Borrowings.EnsureCreated(con);
                Reservations.EnsureCreated(con);
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }
    }
}
