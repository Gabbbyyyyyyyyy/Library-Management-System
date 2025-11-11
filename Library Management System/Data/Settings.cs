using System;
using System.Data.SQLite;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Data
{
    internal static class Settings
    {
        // Ensure the Settings table exists
        public static void EnsureTableExists()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT
                    );", con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Insert default LibraryStatus if it doesn't exist
                using (var cmd = new SQLiteCommand(@"
                    INSERT OR IGNORE INTO Settings (Key, Value) 
                    VALUES ('LibraryStatus', 'Open');", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Get a setting value by key
        public static string Get(string key)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("SELECT Value FROM Settings WHERE Key=@key;", con))
                {
                    cmd.Parameters.AddWithValue("@key", key);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        // Set a setting value by key
        public static void Set(string key, string value)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("UPDATE Settings SET Value=@value WHERE Key=@key;", con))
                {
                    cmd.Parameters.AddWithValue("@key", key);
                    cmd.Parameters.AddWithValue("@value", value);
                    int rows = cmd.ExecuteNonQuery();

                    // If key doesn't exist, insert it
                    if (rows == 0)
                    {
                        using (var insertCmd = new SQLiteCommand("INSERT INTO Settings (Key, Value) VALUES (@key, @value);", con))
                        {
                            insertCmd.Parameters.AddWithValue("@key", key);
                            insertCmd.Parameters.AddWithValue("@value", value);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        // Library-specific helpers
        public static bool IsLibraryOpen()
        {
            string status = Get("LibraryStatus");
            return status == "Open";
        }

        public static void SetLibraryStatus(string status)
        {
            Set("LibraryStatus", status);
        }
    }
}
