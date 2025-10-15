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

        // SQLite connection string
        public static string ConnectionString => $"Data Source={DbPath};Version=3;";

        /// <summary>
        /// Ensures that the database file and tables exist.
        /// ✅ Never overwrites existing DB
        /// ✅ Always uses the actual runtime DB (bin\Debug\Data\library.db)
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
