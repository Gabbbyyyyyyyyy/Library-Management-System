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
        }

        private void LoadBorrowedBooks()
        {
            var dt = DatabaseHelper.Query(@"
                SELECT br.BorrowingID, m.FullName, b.Title, br.BorrowDate, br.DueDate
                FROM Borrowings br
                INNER JOIN Members m ON br.MemberID = m.MemberID
                INNER JOIN Books b ON br.BookID = b.BookID
                WHERE br.ReturnDate IS NULL
            ");
            dgvBorrowedBooks.DataSource = dt;
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            if (dgvBorrowedBooks.SelectedRows.Count == 0)
            {
                lblMessage.Text = "Select a borrowed book to return.";
                return;
            }

            var sel = dgvBorrowedBooks.SelectedRows[0];
            int borrowingId = Convert.ToInt32(sel.Cells["BorrowingID"].Value);
            DateTime dueDate = Convert.ToDateTime(sel.Cells["DueDate"].Value);
            int bookId = GetBookIdFromBorrowing(borrowingId);

            DateTime returnDate = DateTime.Now.Date;
            int penalty = 0;

            if (returnDate > dueDate)
            {
                penalty = (returnDate - dueDate).Days * 5; // Example: ₱5 per day late
            }

            using (var conn = new SQLiteConnection("Data Source=library.db;Version=3;"))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    // Update Borrowings table
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "UPDATE Borrowings SET ReturnDate=@rd, Penalty=@p WHERE BorrowingID=@id";
                        cmd.Parameters.AddWithValue("@rd", returnDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@p", penalty);
                        cmd.Parameters.AddWithValue("@id", borrowingId);
                        cmd.ExecuteNonQuery();
                    }

                    // Increment book availability
                    using (var cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookID=@b";
                        cmd2.Parameters.AddWithValue("@b", bookId);
                        cmd2.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
            }

            lblMessage.Text = penalty > 0
                ? $"Book returned. Penalty: ₱{penalty}"
                : "Book returned successfully.";
            LoadBorrowedBooks();
        }

        private int GetBookIdFromBorrowing(int borrowingId)
        {
            var dt = DatabaseHelper.Query("SELECT BookID FROM Borrowings WHERE BorrowingID = @id",
                new SQLiteParameter("@id", borrowingId));

            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["BookID"]) : -1;
        }
    }
}
