using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using Library_Management_System.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control
{
    public partial class ReturnBooksControl : UserControl
    {
        public ReturnBooksControl()
        {
            InitializeComponent();
            LoadBorrowedBooks();
            // Subscribe to event
            StudentForm.BookReturned += () => LoadBorrowedBooks();
        }

        private void LoadBorrowedBooks()
        {
            var dt = DatabaseHelper.Query(@"
                SELECT br.BorrowId, 
                       m.FirstName || ' ' || m.LastName AS FullName, 
                       b.Title, 
                       br.BorrowDate, 
                       br.DueDate
                FROM Borrowings br
                INNER JOIN Members m ON br.MemberId = m.MemberId
                INNER JOIN Books b ON br.BookId = b.BookId
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
            int borrowId = Convert.ToInt32(sel.Cells["BorrowId"].Value);
            DateTime dueDate = Convert.ToDateTime(sel.Cells["DueDate"].Value);
            int bookId = GetBookIdFromBorrowing(borrowId);

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
                        cmd.CommandText = "UPDATE Borrowings SET ReturnDate=@rd, Penalty=@p WHERE BorrowId=@id";
                        cmd.Parameters.AddWithValue("@rd", returnDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@p", penalty);
                        cmd.Parameters.AddWithValue("@id", borrowId);
                        cmd.ExecuteNonQuery();
                    }

                    // Increment book availability
                    using (var cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId=@b";
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

        private int GetBookIdFromBorrowing(int borrowId)
        {
            var dt = DatabaseHelper.Query("SELECT BookId FROM Borrowings WHERE BorrowId = @id",
                new SQLiteParameter("@id", borrowId));

            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["BookId"]) : -1;
        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void ReturnBooksControl_Load(object sender, EventArgs e)
        {

        }
    }
}
