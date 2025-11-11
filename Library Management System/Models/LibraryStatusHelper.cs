using System;
using System.Data.SQLite;
using LibraryManagementSystem.Data; // wherever Db class is


namespace Library_Management_System.Models
{
    internal static class LibraryStatusHelper
    {
        /// <summary>
        /// Get the current library status from the Settings table ("Open" or "Closed")
        /// </summary>
        /// <returns>Library status as string</returns> 
        public static string GetLibraryStatus()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("SELECT Value FROM Settings WHERE Key='LibraryStatus';", con))
                {
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "Open";
                }
            }
        }

        /// <summary>
        /// Set the library status in the Settings table
        /// </summary>
        /// <param name="status">"Open" or "Closed"</param>
        public static void SetLibraryStatus(string status)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("UPDATE Settings SET Value=@v WHERE Key='LibraryStatus';", con))
                {
                    cmd.Parameters.AddWithValue("@v", status);
                    int rows = cmd.ExecuteNonQuery();

                    // If the key doesn't exist, insert it
                    if (rows == 0)
                    {
                        using (var insertCmd = new SQLiteCommand("INSERT INTO Settings (Key, Value) VALUES ('LibraryStatus', @v);", con))
                        {
                            insertCmd.Parameters.AddWithValue("@v", status);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the library is currently open
        /// </summary>
        public static bool IsLibraryOpen()
        {
            return GetLibraryStatus() == "Open";
        }
    }
}
