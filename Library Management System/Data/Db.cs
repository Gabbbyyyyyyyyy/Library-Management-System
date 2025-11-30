using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using Library_Management_System.Data;

namespace LibraryManagementSystem.Data
{
    public static class Db
    {
        // 🔒 Always use the bin\Debug\Data\library.db file
        public static string DbPath => Path.Combine(Application.StartupPath, "Data", "library.db");

        // SQLite connection string (Pooling + Busy Timeout for stability)
        public static string ConnectionString =>
            $"Data Source={DbPath};Version=3;Pooling=True;Max Pool Size=100;BusyTimeout=5000;";

        /// <summary>
        /// Ensures that the database file and tables exist.
        /// ✅ Never overwrites existing DB
        /// ✅ Always uses the actual runtime DB (bin\\Debug\\Data\\library.db)
        /// </summary>
        public static void EnsureCreated()
        {
            // 🧩 Create "Data" folder if missing
            string dataFolder = Path.GetDirectoryName(DbPath);
            if (dataFolder != null && !Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            // 🛑 Do NOT overwrite if DB already exists
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            // 🗃️ Make sure tables exist (won’t drop existing data)
            using (var con = GetConnection())
            {
                con.Open();

                // ⚙️ Enable Write-Ahead Logging (allows concurrent reads/writes)
                using (var pragma = new SQLiteCommand("PRAGMA journal_mode=WAL;", con))
                    pragma.ExecuteNonQuery();

                using (var pragma2 = new SQLiteCommand("PRAGMA synchronous=NORMAL;", con))
                    pragma2.ExecuteNonQuery();

                Members.EnsureCreated(con);
                Books.EnsureCreated(con);
                Borrowings.EnsureCreated(con);
                Reservations.EnsureCreated(con);
                Penalties.EnsureCreated(con);
                Notifications.EnsureCreated(con);




                // ✅ Ensure indexes exist on Borrowings table
                var cmd = con.CreateCommand();
                cmd.CommandText = @"
                    CREATE INDEX IF NOT EXISTS idx_borrow_date ON Borrowings(BorrowDate);
                    CREATE INDEX IF NOT EXISTS idx_return_date ON Borrowings(ReturnDate);
                    CREATE INDEX IF NOT EXISTS idx_due_date ON Borrowings(DueDate);
                ";
                cmd.ExecuteNonQuery();
            }
        }

        public static SQLiteConnection GetConnection()
        {
            var con = new SQLiteConnection(ConnectionString);

            // When connection opens, ensure WAL mode and busy timeout
            con.Open();
            using (var pragma = new SQLiteCommand("PRAGMA journal_mode=WAL;", con))
                pragma.ExecuteNonQuery();

            using (var pragma2 = new SQLiteCommand("PRAGMA busy_timeout = 5000;", con))
                pragma2.ExecuteNonQuery();

            con.Close();
            return con;
        }
    }
}
