using System;
using System.Data.SQLite;
using Library_Management_System.Models;
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

            if (!LibraryStatusHelper.IsLibraryOpen())
                return; // ✅ Skip penalty updates when library is closed

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Ensure HoursOverdue column exists
                EnsureColumnExists(con, "Penalties", "HoursOverdue", "INTEGER DEFAULT 0");

                var now = DateTime.Now;
                string query = @"
                    SELECT BorrowId, MemberId, BorrowDate, DueDate, ReturnDate
                    FROM Borrowings
                    WHERE (ReturnDate IS NOT NULL AND ReturnDate > DueDate)
                       OR (ReturnDate IS NULL AND DueDate < @now)
                ";
                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@now", now);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int borrowId = Convert.ToInt32(reader["BorrowId"]);
                            int memberId = Convert.ToInt32(reader["MemberId"]);
                            DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                            DateTime? returnDate = reader["ReturnDate"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["ReturnDate"]);

                        

                            // --------------------------
                            // Calculate penalty
                            // --------------------------
                            double penaltyAmount;
                            int daysOverdue, hoursOverdue;

                            PenaltyHelper.CalculatePenalty(dueDate, returnDate,
                                out penaltyAmount, out daysOverdue, out hoursOverdue);

                            // If still in grace period (no penalty yet)
                            if (penaltyAmount <= 0)
                                continue;

                            // --------------------------
                            // Insert or update Penalties table
                            // --------------------------
                            string checkSql = "SELECT COUNT(*) FROM Penalties WHERE BorrowId=@borrowId";
                            using (var checkCmd = new SQLiteCommand(checkSql, con))
                            {
                                checkCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                long exists = (long)checkCmd.ExecuteScalar();

                                if (exists == 0)
                                {
                                    string insertSql = @"
                            INSERT INTO Penalties (BorrowId, MemberId, Amount, DaysOverdue, HoursOverdue, Status)
                            VALUES (@borrowId, @memberId, @amount, @daysOverdue, @hoursOverdue, 'Unpaid')
                        ";
                                    using (var insertCmd = new SQLiteCommand(insertSql, con))
                                    {
                                        insertCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                        insertCmd.Parameters.AddWithValue("@memberId", memberId);
                                        insertCmd.Parameters.AddWithValue("@amount", penaltyAmount);
                                        insertCmd.Parameters.AddWithValue("@daysOverdue", daysOverdue);
                                        insertCmd.Parameters.AddWithValue("@hoursOverdue", hoursOverdue);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    string updateSql = @"
                            UPDATE Penalties
                            SET Amount=@amount, DaysOverdue=@daysOverdue, HoursOverdue=@hoursOverdue
                            WHERE BorrowId=@borrowId AND Status='Unpaid'
                        ";
                                    using (var updateCmd = new SQLiteCommand(updateSql, con))
                                    {
                                        updateCmd.Parameters.AddWithValue("@amount", penaltyAmount);
                                        updateCmd.Parameters.AddWithValue("@daysOverdue", daysOverdue);
                                        updateCmd.Parameters.AddWithValue("@hoursOverdue", hoursOverdue);
                                        updateCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                        updateCmd.ExecuteNonQuery();
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
