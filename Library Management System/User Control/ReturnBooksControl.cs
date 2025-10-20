using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control
{
    public partial class ReturnBooksControl : UserControl
    {
        public ReturnBooksControl()
        {
            InitializeComponent();
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
            double penalty = Convert.ToDouble(sel.Cells["Penalty"].Value);

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    // Update Borrowings status and penalty
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "UPDATE Borrowings SET Status='Accepted', Penalty=@penalty WHERE BorrowId=@id";
                        cmd.Parameters.AddWithValue("@id", borrowId);
                        cmd.Parameters.AddWithValue("@penalty", penalty);
                        cmd.ExecuteNonQuery();
                    }

                    // Increment available copies
                    using (var cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId=@b";
                        cmd2.Parameters.AddWithValue("@b", bookId);
                        cmd2.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
            }

            MessageBox.Show("✅ Book return accepted! Penalty recorded: ₱" + penalty);
            LoadBorrowedBooks();
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
    }
}
