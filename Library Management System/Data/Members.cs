using System;
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
                YearLevel  TEXT,
                DateJoined TEXT,
                IsActive   INTEGER DEFAULT 1
            );";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }

            // ✅ Check and add DateJoined if missing
            bool hasDateJoined = false;
            bool hasIsActive = false;

            using (var cmd = new SQLiteCommand("PRAGMA table_info(Members);", con))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string columnName = reader["name"].ToString();
                    if (columnName.Equals("DateJoined", StringComparison.OrdinalIgnoreCase))
                        hasDateJoined = true;
                    else if (columnName.Equals("IsActive", StringComparison.OrdinalIgnoreCase))
                        hasIsActive = true;
                }
            }

            if (!hasDateJoined)
            {
                using (var cmd = new SQLiteCommand("ALTER TABLE Members ADD COLUMN DateJoined TEXT;", con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Optional: initialize existing members with current date
                using (var cmd = new SQLiteCommand("UPDATE Members SET DateJoined = DATE('now') WHERE DateJoined IS NULL;", con))
                {
                    cmd.ExecuteNonQuery();
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
