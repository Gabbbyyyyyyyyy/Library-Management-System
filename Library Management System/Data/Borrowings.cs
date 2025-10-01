using System.Data.SQLite;

namespace LibraryManagementSystem.Data
{
    public static class Borrowings
    {
        public static void EnsureCreated(SQLiteConnection con)
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS Borrowings (
                BorrowId   INTEGER PRIMARY KEY AUTOINCREMENT,
                MemberId   INTEGER NOT NULL,
                BookId     INTEGER NOT NULL,
                BorrowDate TEXT NOT NULL,
                DueDate    TEXT NOT NULL,
                ReturnDate TEXT DEFAULT NULL,
                Penalty    REAL DEFAULT 0,
                FOREIGN KEY(MemberId) REFERENCES Members(MemberId),
                FOREIGN KEY(BookId) REFERENCES Books(BookId)
            );";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
