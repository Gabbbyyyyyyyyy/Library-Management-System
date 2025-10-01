using System.Data.SQLite;

namespace LibraryManagementSystem.Data
{
    public static class Members
    {
        public static void EnsureCreated(SQLiteConnection con)
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS Members (
                MemberId   INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentNo  TEXT UNIQUE,
                FirstName  TEXT NOT NULL,
                LastName   TEXT NOT NULL,
                Course     TEXT,
                YearLevel  TEXT
            );";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }

            // ✅ Check if IsActive exists
            bool hasIsActive = false;
            using (var cmd = new SQLiteCommand("PRAGMA table_info(Members);", con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString().Equals("IsActive", System.StringComparison.OrdinalIgnoreCase))
                    {
                        hasIsActive = true;
                        break;
                    }
                }
            }

            if (!hasIsActive)
            {
                using (var cmd = new SQLiteCommand("ALTER TABLE Members ADD COLUMN IsActive INTEGER DEFAULT 1;", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
