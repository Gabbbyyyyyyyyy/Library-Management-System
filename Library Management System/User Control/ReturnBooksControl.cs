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

            // Subscribe to student event
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
                WHERE br.ReturnDate IS NOT NULL AND br.Status = 'Pending'
            ");
            dgvBorrowedBooks.DataSource = dt;
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

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    // Update Borrowings status to Accepted
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "UPDATE Borrowings SET Status='Accepted' WHERE BorrowId=@id";
                        cmd.Parameters.AddWithValue("@id", borrowId);
                        cmd.ExecuteNonQuery();
                    }

                    // Increment AvailableCopies
                    using (var cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId=@b";
                        cmd2.Parameters.AddWithValue("@b", bookId);
                        cmd2.ExecuteNonQuery();
                    }

                    // Update student HasPendingBorrow if tracked
                    using (var cmd3 = new SQLiteCommand(conn))
                    {
                        cmd3.CommandText = @"
                            UPDATE Members
                            SET HasPendingBorrow = (
                                SELECT COUNT(*) 
                                FROM Borrowings 
                                WHERE MemberId = Members.MemberId AND ReturnDate IS NULL
                            )
                            WHERE MemberId = (SELECT MemberId FROM Borrowings WHERE BorrowId=@id)";
                        cmd3.Parameters.AddWithValue("@id", borrowId);
                        cmd3.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
            }

            LoadBorrowedBooks(); // Refresh grid
        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

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

            // Get BookId of the selected borrow
            int bookId = GetBookIdFromBorrowing(borrowId);
            if (bookId == -1)
            {
                MessageBox.Show("❌ Unable to find the book record for this borrowing.");
                return;
            }

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Update Borrowing status to 'Returned'
                        using (var cmd = new SQLiteCommand("UPDATE Borrowings SET Status = 'Returned' WHERE BorrowId = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        // Step 2: Update Book's AvailableCopies
                        using (var cmd2 = new SQLiteCommand("UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId = @b", conn))
                        {
                            cmd2.Parameters.AddWithValue("@b", bookId);
                            cmd2.ExecuteNonQuery();
                        }

                        // Step 3: Commit changes
                        trans.Commit();

                        // Step 4: Remove the book from DataGridView
                        dgvBorrowedBooks.Rows.Remove(selectedRow);

                        MessageBox.Show("✅ Book successfully returned and stock updated!");
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
