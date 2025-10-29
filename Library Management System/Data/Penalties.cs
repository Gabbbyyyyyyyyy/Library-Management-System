using System.Data.SQLite;

namespace LibraryManagementSystem.Data
{
    public static class Penalties
    {
        public static void EnsureCreated(SQLiteConnection con)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Penalties (
                    PenaltyId     INTEGER PRIMARY KEY AUTOINCREMENT,
                    BorrowId      INTEGER NOT NULL,
                    MemberId      INTEGER NOT NULL,
                    Amount        REAL NOT NULL,
                    DaysOverdue   INTEGER NOT NULL,
                    CreatedAt     DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (BorrowId) REFERENCES Borrowings(BorrowId),
                    FOREIGN KEY (MemberId) REFERENCES Members(MemberId)
                );
            ";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
