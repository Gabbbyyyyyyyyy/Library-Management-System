using System.Data.SQLite;

namespace LibraryManagementSystem.Data
{
    //DROP TABLE IF EXISTS Books; - to resest all the data of books
    public static class Books
    {
        public static void EnsureCreated(SQLiteConnection con)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Books (
                    BookId          INTEGER PRIMARY KEY AUTOINCREMENT,
                    ISBN            TEXT,
                    Title           TEXT NOT NULL,
                    Author          TEXT,
                    Category        TEXT,
                    Quantity        INTEGER NOT NULL DEFAULT 1,
                    AvailableCopies INTEGER NOT NULL DEFAULT 1,
                    CreatedAt       DATETIME DEFAULT CURRENT_TIMESTAMP
                );";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
