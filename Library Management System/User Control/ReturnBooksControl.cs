using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Helpers;

namespace Library_Management_System.User_Control
{
    public partial class ReturnBooksControl : UserControl
    {
        public ReturnBooksControl()
        {
            InitializeComponent();
            // 🔹 Set column header font to size 10
            dgvBorrowedBooks.EnableHeadersVisualStyles = false; // ensures your font change is applied
            dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);


            LoadBorrowedBooks();

            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            Library_Management_System.User_Control_Student.Borrowing.BookReturned += LoadBorrowedBooks;
        }

        private void LoadBorrowedBooks()
        {
            var dt = DatabaseHelper.Query(@"
                SELECT br.BorrowId, 
                       m.FirstName || ' ' || m.LastName AS FullName, 
                       b.Title, 
                       br.BorrowDate, 
                       br.DueDate,
                       br.ReturnDate,
                       br.Penalty,
                       br.Status
                FROM Borrowings br
                INNER JOIN Members m ON br.MemberId = m.MemberId
                INNER JOIN Books b ON br.BookId = b.BookId
                WHERE br.Status = 'Pending'
                ORDER BY br.DueDate ASC;
            ");

            // Compute penalties dynamically
            foreach (DataRow row in dt.Rows)
            {
                if (row["DueDate"] != DBNull.Value)
                {
                    DateTime dueDate = Convert.ToDateTime(row["DueDate"]);
                    DateTime returnDate = row["ReturnDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(row["ReturnDate"]);

                    // Fine starts 1 hour after due date
                    DateTime fineStart = dueDate.AddHours(1);

                    double totalPenalty = 0;

                    if (returnDate > fineStart)
                    {
                        TimeSpan diff = returnDate - fineStart;

                        int totalHours = (int)Math.Floor(diff.TotalHours);
                        int totalDays = (int)Math.Floor(diff.TotalDays);

                        totalPenalty = (totalHours * 2) + (totalDays * 10);
                    }

                    row["Penalty"] = totalPenalty;
                }
            }

            dgvBorrowedBooks.DataSource = dt;

            // Optional: Auto-format Penalty column
            if (dgvBorrowedBooks.Columns.Contains("Penalty"))
            {
                dgvBorrowedBooks.Columns["Penalty"].DefaultCellStyle.Format = "₱0.00";
                dgvBorrowedBooks.Columns["Penalty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            dgvBorrowedBooks.ClearSelection();
        }

        private int GetBookIdFromBorrowing(int borrowId)
        {
            var dt = DatabaseHelper.Query("SELECT BookId FROM Borrowings WHERE BorrowId = @id",
                new SQLiteParameter("@id", borrowId));

            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["BookId"]) : -1;
        }

        private void BtnAcceptReturn_Click(object sender, EventArgs e)
        {
            if (dgvBorrowedBooks.SelectedRows.Count == 0) return;

            var sel = dgvBorrowedBooks.SelectedRows[0];
            int borrowId = Convert.ToInt32(sel.Cells["BorrowId"].Value);
            int bookId = GetBookIdFromBorrowing(borrowId);

            DateTime dueDate = Convert.ToDateTime(sel.Cells["DueDate"].Value);
            DateTime returnDate = DateTime.Now;

            // ✅ Use PenaltyHelper
            PenaltyHelper.CalculatePenalty(dueDate, returnDate, out double totalPenalty, out int daysOverdue);

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Update Borrowings table
                        using (var cmd = new SQLiteCommand("UPDATE Borrowings SET Status='Accepted', ReturnDate=@r, Penalty=@p WHERE BorrowId=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", borrowId);
                            cmd.Parameters.AddWithValue("@r", returnDate);
                            cmd.Parameters.AddWithValue("@p", totalPenalty);
                            cmd.ExecuteNonQuery();
                        }

                        // Insert penalty record only if >0 and not exists
                        if (totalPenalty > 0)
                        {
                            int memberId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                                "SELECT MemberId FROM Borrowings WHERE BorrowId=@id",
                                new SQLiteParameter("@id", borrowId)
                            ));

                            var existing = DatabaseHelper.Query(
                                "SELECT COUNT(1) FROM Penalties WHERE BorrowId=@id",
                                new SQLiteParameter("@id", borrowId)
                            );

                            if (Convert.ToInt32(existing.Rows[0][0]) == 0)
                            {
                                using (var cmd2 = new SQLiteCommand(conn))
                                {
                                    cmd2.CommandText = @"
                                        INSERT INTO Penalties (BorrowId, MemberId, Amount, DaysOverdue)
                                        VALUES (@borrowId, @memberId, @amount, @days)";
                                    cmd2.Parameters.AddWithValue("@borrowId", borrowId);
                                    cmd2.Parameters.AddWithValue("@memberId", memberId);
                                    cmd2.Parameters.AddWithValue("@amount", totalPenalty);
                                    cmd2.Parameters.AddWithValue("@days", daysOverdue);
                                    cmd2.ExecuteNonQuery();
                                }
                            }
                        }

                        // Increment available copies
                        using (var cmd3 = new SQLiteCommand("UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId=@b", conn))
                        {
                            cmd3.Parameters.AddWithValue("@b", bookId);
                            cmd3.ExecuteNonQuery();
                        }

                        trans.Commit();
                        MessageBox.Show("✅ Book return accepted! Penalty recorded: ₱" + totalPenalty.ToString("0.00"));
                        LoadBorrowedBooks();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("❌ Error while recording return:\n" + ex.Message);
                    }
                }
            }
        }
    

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (dgvBorrowedBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("⚠️ Please select a book to mark as returned.");
                return;
            }

            var selectedRow = dgvBorrowedBooks.SelectedRows[0];
            int borrowId = Convert.ToInt32(selectedRow.Cells["BorrowId"].Value);
            int bookId = GetBookIdFromBorrowing(borrowId);

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SQLiteCommand("UPDATE Borrowings SET Status = 'Returned', ReturnDate = @r WHERE BorrowId = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@r", DateTime.Now);
                            cmd.Parameters.AddWithValue("@id", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd2 = new SQLiteCommand("UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId = @b", conn))
                        {
                            cmd2.Parameters.AddWithValue("@b", bookId);
                            cmd2.ExecuteNonQuery();
                        }

                        trans.Commit();
                        dgvBorrowedBooks.Rows.Remove(selectedRow);

                        MessageBox.Show("✅ Book successfully marked as returned!");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("❌ Error while returning book:\n" + ex.Message);
                    }
                }
            }
        }

        private void lblMessage_Click(object sender, EventArgs e)
        {

        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
