using System.Data.SQLite;

namespace Library_Management_System.Data
{
    public static class Reservations
    {
        // Ensures the Reservations table exists (creates if not)
        public static void EnsureCreated(SQLiteConnection con)
        {
            string sql = @"
            CREATE TABLE IF NOT EXISTS Reservations (
                ReservationId INTEGER PRIMARY KEY AUTOINCREMENT,
                BookId        INTEGER NOT NULL,
                MemberId      INTEGER NOT NULL,
                ReserveDate   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                Status        TEXT NOT NULL CHECK (Status IN ('Active', 'Cancelled', 'Fulfilled')),

                FOREIGN KEY (BookId) REFERENCES Books(BookId) ON DELETE CASCADE,
                FOREIGN KEY (MemberId) REFERENCES Members(MemberId) ON DELETE CASCADE
            );";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }

            // Optional: add indexes for faster lookups
            string indexSql1 = @"CREATE INDEX IF NOT EXISTS idx_reservations_bookid ON Reservations(BookId);";
            string indexSql2 = @"CREATE INDEX IF NOT EXISTS idx_reservations_memberid ON Reservations(MemberId);";

            using (var cmd = new SQLiteCommand(indexSql1, con))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(indexSql2, con))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
