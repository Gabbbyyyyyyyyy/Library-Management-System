using System;
using System.Data.SQLite;
using LibraryManagementSystem.Helpers;

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
                    Status        TEXT DEFAULT 'Unpaid',
                    PaidDate      DATETIME,
                    CreatedAt     DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (BorrowId) REFERENCES Borrowings(BorrowId),
                    FOREIGN KEY (MemberId) REFERENCES Members(MemberId)
                );
            ";

            using (var cmd = new SQLiteCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }

            // 🧩 Optional: make sure new columns exist for older DBs
            EnsureColumnExists(con, "Penalties", "Status", "TEXT DEFAULT 'Unpaid'");
            EnsureColumnExists(con, "Penalties", "PaidDate", "DATETIME");
        }

        private static void EnsureColumnExists(SQLiteConnection con, string table, string column, string definition)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({table});", con))
            using (var reader = cmd.ExecuteReader())
            {
                bool exists = false;
                while (reader.Read())
                {
                    if (reader["name"].ToString() == column)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    using (var alterCmd = new SQLiteCommand($"ALTER TABLE {table} ADD COLUMN {column} {definition};", con))
                    {
                        alterCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void SyncPenaltiesFromBorrowings()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                // Only handle unpaid penalties dynamically
                string query = @"
            SELECT BorrowId, MemberId, Penalty, BorrowDate, DueDate, ReturnDate
            FROM Borrowings
            WHERE Penalty > 0 AND BorrowId NOT IN (SELECT BorrowId FROM Penalties WHERE Status = 'Paid')
        ";

                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int borrowId = Convert.ToInt32(reader["BorrowId"]);
                        int memberId = Convert.ToInt32(reader["MemberId"]);
                        double penalty = Convert.ToDouble(reader["Penalty"]);
                        DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                        DateTime? returnDate = reader["ReturnDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReturnDate"]);

                        int daysOverdue = (int)Math.Ceiling(((returnDate ?? DateTime.Now) - dueDate).TotalDays);

                        if (daysOverdue > 0)
                        {
                            // Insert new unpaid penalty if not exists
                            string checkSql = "SELECT COUNT(*) FROM Penalties WHERE BorrowId = @borrowId";
                            using (var checkCmd = new SQLiteCommand(checkSql, con))
                            {
                                checkCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                long exists = (long)checkCmd.ExecuteScalar();

                                if (exists == 0)
                                {
                                    string insertSql = @"
                                INSERT INTO Penalties (BorrowId, MemberId, Amount, DaysOverdue, Status)
                                VALUES (@borrowId, @memberId, @amount, @daysOverdue, 'Unpaid')
                            ";
                                    using (var insertCmd = new SQLiteCommand(insertSql, con))
                                    {
                                        insertCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                        insertCmd.Parameters.AddWithValue("@memberId", memberId);
                                        insertCmd.Parameters.AddWithValue("@amount", penalty);
                                        insertCmd.Parameters.AddWithValue("@daysOverdue", daysOverdue);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static (double totalPaidThisMonth, double totalUnpaid) GetPenaltySummary()
        {
            double totalPaidThisMonth = 0;
            double totalUnpaid = 0;

            using (var con = Db.GetConnection())
            {
                con.Open();

                string sql = @"
            SELECT 
                SUM(CASE WHEN Status = 'Paid' AND strftime('%Y-%m', PaidDate) = @period THEN Amount ELSE 0 END) AS TotalPaid,
                SUM(CASE WHEN Status = 'Unpaid' THEN Amount ELSE 0 END) AS TotalUnpaid
            FROM Penalties
        ";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    // e.g., "2025-11" for November 2025
                    string period = DateTime.Now.ToString("yyyy-MM");
                    cmd.Parameters.AddWithValue("@period", period);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            totalPaidThisMonth = reader["TotalPaid"] != DBNull.Value ? Convert.ToDouble(reader["TotalPaid"]) : 0;
                            totalUnpaid = reader["TotalUnpaid"] != DBNull.Value ? Convert.ToDouble(reader["TotalUnpaid"]) : 0;
                        }
                    }
                }
            }

            return (totalPaidThisMonth, totalUnpaid);
        }



    }
}
