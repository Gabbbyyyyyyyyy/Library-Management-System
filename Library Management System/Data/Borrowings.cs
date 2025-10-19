using System;
using System.Data.SQLite;

namespace LibraryManagementSystem.Data
{
    public static class Borrowings
    {
        public static void EnsureCreated(SQLiteConnection con)
        {
            // Step 1: Create the table if it doesn't exist
            string createTableSql = @"
            CREATE TABLE IF NOT EXISTS Borrowings (
                BorrowId   INTEGER PRIMARY KEY AUTOINCREMENT,
                MemberId   INTEGER NOT NULL,
                BookId     INTEGER NOT NULL,
                BorrowDate TEXT NOT NULL,
                DueDate    TEXT NOT NULL,
                ReturnDate TEXT DEFAULT NULL,
                Penalty    REAL DEFAULT 0,
                Status     TEXT DEFAULT 'Borrowed',
                FOREIGN KEY(MemberId) REFERENCES Members(MemberId),
                FOREIGN KEY(BookId) REFERENCES Books(BookId)
            );";

            using (var cmd = new SQLiteCommand(createTableSql, con))
            {
                cmd.ExecuteNonQuery();
            }

            // Step 2: Check if the 'Status' column already exists
            bool hasStatusColumn = false;
            using (var checkCmd = new SQLiteCommand("PRAGMA table_info(Borrowings);", con))
            using (var reader = checkCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString().Equals("Status", StringComparison.OrdinalIgnoreCase))
                    {
                        hasStatusColumn = true;
                        break;
                    }
                }
            }

            // Step 3: Add the column if missing
            if (!hasStatusColumn)
            {
                string alterSql = "ALTER TABLE Borrowings ADD COLUMN Status TEXT DEFAULT 'Borrowed';";
                using (var alterCmd = new SQLiteCommand(alterSql, con))
                {
                    alterCmd.ExecuteNonQuery();
                    Console.WriteLine("✅ 'Status' column added to Borrowings table.");
                }
            }
        }

        public static void BorrowBook(int memberId, int bookId, string dueDate)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                // Insert new borrowing record with the current timestamp for BorrowDate
                string query = @"
            INSERT INTO Borrowings (MemberId, BookId, BorrowDate, DueDate, Status)
            VALUES (@MemberId, @BookId, CURRENT_TIMESTAMP, @DueDate, 'Borrowed');
        ";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate);  // Example due date, could be calculated

                    cmd.ExecuteNonQuery();
                }

            }
        }

        public static void ReturnBook(int borrowId)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                    UPDATE Borrowings
                    SET Status = 'Returned', ReturnDate = CURRENT_TIMESTAMP
                    WHERE BorrowId = @BorrowId";


                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@BorrowId", borrowId);
                    cmd.ExecuteNonQuery();
                }
            }
        }


    }
}


// If you add new columns in the future (like Remarks, LibrarianName, etc.),
//you can follow the same logic — just copy the “Step 2 + Step 3” pattern.
