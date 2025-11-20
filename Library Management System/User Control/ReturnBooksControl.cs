using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Helpers;

namespace Library_Management_System.User_Control
{
    public partial class ReturnBooksControl : UserControl
    {
        public event Action PenaltyUpdated;
        private int targetWidth = 150; // final width when fully shown
        private Timer slideTimer;


        public ReturnBooksControl()
        {
            InitializeComponent();
            // Initially collapsed
            panel1.Width = 0;

            // Setup Timer
            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;
            dgvBorrowedBooks.EnableHeadersVisualStyles = false;
            dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            // 🔹 Make rows taller
            dgvBorrowedBooks.RowTemplate.Height = 40; // increase row height (default is ~22)


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

            foreach (DataRow row in dt.Rows)
            {
                if (row["DueDate"] != DBNull.Value)
                {
                    DateTime dueDate = Convert.ToDateTime(row["DueDate"]);
                    DateTime? returnDate = row["ReturnDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ReturnDate"]);

                    double totalPenalty;
                    int daysOverdue;
                    int hoursOverdue;

                    PenaltyHelper.CalculatePenalty(dueDate, returnDate, out totalPenalty, out daysOverdue, out hoursOverdue);
                    row["Penalty"] = totalPenalty;

                }
            }

            dgvBorrowedBooks.DataSource = dt;
            dgvBorrowedBooks.Columns["FullName"].HeaderText = "Name of Students";
            dgvBorrowedBooks.Columns["BorrowDate"].HeaderText = "Borrow Date";
            dgvBorrowedBooks.Columns["ReturnDate"].HeaderText = "Return Date";
            dgvBorrowedBooks.Columns["DueDate"].HeaderText = "Due Date";
            // Hide MemberId column
            if (dgvBorrowedBooks.Columns.Contains("BorrowId"))
                dgvBorrowedBooks.Columns["BorrowId"].Visible = false;


            if (dgvBorrowedBooks.Columns.Contains("Penalty"))
            {
                dgvBorrowedBooks.Columns["Penalty"].DefaultCellStyle.Format = "₱0.00";
                dgvBorrowedBooks.Columns["Penalty"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            dgvBorrowedBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // auto-fit columns
                                                                                         // 🔹 Remove pre-selection completely
            dgvBorrowedBooks.ClearSelection();
            dgvBorrowedBooks.CurrentCell = null;
        }

        private int GetBookIdFromBorrowing(int borrowId)
        {
            var dt = DatabaseHelper.Query("SELECT BookId FROM Borrowings WHERE BorrowId = @id",
                new SQLiteParameter("@id", borrowId));

            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["BookId"]) : -1;
        }

        private void BtnAcceptReturn_Click(object sender, EventArgs e)
        {

            // ✅ Check library status
            if (!LibraryStatusHelper.IsLibraryOpen())
            {
                MessageBox.Show("The library is currently closed. Transactions are paused.",
                                "Library Closed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvBorrowedBooks.SelectedRows.Count == 0) return;

            var sel = dgvBorrowedBooks.SelectedRows[0];
            int borrowId = Convert.ToInt32(sel.Cells["BorrowId"].Value);
            int bookId = GetBookIdFromBorrowing(borrowId);

            DateTime dueDate = Convert.ToDateTime(sel.Cells["DueDate"].Value);
            DateTime returnDate = DateTime.Now;

            double totalPenalty;
            int daysOverdue;
            int hoursOverdue;

            PenaltyHelper.CalculatePenalty(dueDate, returnDate, out totalPenalty, out daysOverdue, out hoursOverdue);


            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SQLiteCommand("UPDATE Borrowings SET Status='Accepted', ReturnDate=@r, Penalty=@p WHERE BorrowId=@id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", borrowId);
                            cmd.Parameters.AddWithValue("@r", returnDate);
                            cmd.Parameters.AddWithValue("@p", totalPenalty);
                            cmd.ExecuteNonQuery();
                        }

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

                        using (var cmd3 = new SQLiteCommand("UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId=@b", conn))
                        {
                            cmd3.Parameters.AddWithValue("@b", bookId);
                            cmd3.ExecuteNonQuery();
                        }

                        trans.Commit();
                        MessageBox.Show($"✅ Book return accepted!\nPenalty recorded: ₱{totalPenalty:0.00}");
                        LoadBorrowedBooks();

                        PenaltyUpdated?.Invoke();
                        Penalties.SyncPenaltiesFromBorrowings();

                        var dashboard = this.Parent?.Controls["DashboardControl"] as DashboardControl;
                        dashboard?.RefreshPenaltySummary();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("⚠️ Something went wrong while processing the return.\n\nDetails: " + ex.Message);
                    }
                }
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {

            // ✅ Block marking returns if library is closed
            if (!LibraryStatusHelper.IsLibraryOpen())
            {
                MessageBox.Show("The library is currently closed. Transactions are paused.",
                                "Library Closed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvBorrowedBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("⚠️ Please select a book to mark as returned.");
                return;
            }

            if (MessageBox.Show("Confirm marking this book as returned?", "Confirm Return",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

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
                        // ✅ 1. Mark as returned
                        using (var cmd = new SQLiteCommand(
                            "UPDATE Borrowings SET Status = 'Returned', ReturnDate = @r WHERE BorrowId = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@r", DateTime.Now);
                            cmd.Parameters.AddWithValue("@id", borrowId);
                            cmd.ExecuteNonQuery();
                        }

                        // ✅ 2. Update book availability
                        using (var cmd2 = new SQLiteCommand(
                            "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId = @b", conn))
                        {
                            cmd2.Parameters.AddWithValue("@b", bookId);
                            cmd2.ExecuteNonQuery();
                        }

                        // ✅ 3. Get member and due date
                        int memberId;
                        DateTime dueDate;
                        using (var cmdMember = new SQLiteCommand(
                            "SELECT MemberId, DueDate FROM Borrowings WHERE BorrowId = @id", conn))
                        {
                            cmdMember.Parameters.AddWithValue("@id", borrowId);
                            using (var reader = cmdMember.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new Exception("Borrowing not found.");

                                memberId = Convert.ToInt32(reader["MemberId"]);
                                dueDate = Convert.ToDateTime(reader["DueDate"]);
                            }
                        }

                        // ✅ 4. Calculate penalty, days, and hours overdue
                        double penalty;
                        int daysOverdue;
                        int hoursOverdue;
                        PenaltyHelper.CalculatePenalty(dueDate, DateTime.Now, out penalty, out daysOverdue, out hoursOverdue);

                        // ✅ 5. Always update Borrowings.Penalty field
                        using (var updateBorrowing = new SQLiteCommand(
                            "UPDATE Borrowings SET Penalty = @p WHERE BorrowId = @id", conn))
                        {
                            updateBorrowing.Parameters.AddWithValue("@p", penalty);
                            updateBorrowing.Parameters.AddWithValue("@id", borrowId);
                            updateBorrowing.ExecuteNonQuery();
                        }

                        // ✅ 6. Handle overdue penalty record (only if overdue)
                        if (daysOverdue > 0 || hoursOverdue > 0)
                        {
                            using (var checkCmd = new SQLiteCommand(
                                "SELECT COUNT(*) FROM Penalties WHERE BorrowId = @borrowId", conn))
                            {
                                checkCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                long exists = (long)checkCmd.ExecuteScalar();

                                if (exists == 0)
                                {
                                    using (var insertCmd = new SQLiteCommand(@"
                                INSERT INTO Penalties 
                                (BorrowId, MemberId, Amount, DaysOverdue, HoursOverdue, Status, PaidDate)
                                VALUES (@borrowId, @memberId, @amount, @days, @hours, 'Paid', @paidDate)", conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                        insertCmd.Parameters.AddWithValue("@memberId", memberId);
                                        insertCmd.Parameters.AddWithValue("@amount", penalty);
                                        insertCmd.Parameters.AddWithValue("@days", daysOverdue);
                                        insertCmd.Parameters.AddWithValue("@hours", hoursOverdue);
                                        insertCmd.Parameters.AddWithValue("@paidDate", DateTime.Now);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    using (var updateCmd = new SQLiteCommand(@"
                                UPDATE Penalties
                                SET Status = 'Paid',
                                    PaidDate = @paidDate,
                                    DaysOverdue = @days,
                                    HoursOverdue = @hours,
                                    Amount = @amount
                                WHERE BorrowId = @borrowId", conn))
                                    {
                                        updateCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                        updateCmd.Parameters.AddWithValue("@days", daysOverdue);
                                        updateCmd.Parameters.AddWithValue("@hours", hoursOverdue);
                                        updateCmd.Parameters.AddWithValue("@paidDate", DateTime.Now);
                                        updateCmd.Parameters.AddWithValue("@amount", penalty);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        // ✅ 7. Always commit the transaction, even if no penalty
                        trans.Commit();

                        // ✅ 8. Update UI
                        dgvBorrowedBooks.Rows.Remove(selectedRow);
                        MessageBox.Show($"✅ Book successfully marked as returned!\n\nPenalty: ₱{penalty:0.00}\nDays Overdue: {daysOverdue}\nHours Overdue: {hoursOverdue}");

                        var dashboard = this.Parent?.Controls["DashboardControl"] as DashboardControl;
                        dashboard?.RefreshPenaltySummary();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("⚠️ Something went wrong while processing the return.\n\nDetails: " + ex.Message);
                    }
                }
            }
        }


        private void lblMessage_Click(object sender, EventArgs e) { }
        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void ReturnBooksControl_Load(object sender, EventArgs e) { }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            if (panel1.Width < targetWidth)
            {
                panel1.Width += 8; // adjust speed
                panel1.Invalidate(); // redraw rounded corners
            }
            else
            {
                slideTimer.Stop(); // stop when fully expanded
            }
        }

        // Call this from MainForm
        public void RollOutPanel()
        {
            // Reset panel to collapsed before rolling out
            if (panel1.Width != 0)
            {
                panel1.Width = 0;
                panel1.Invalidate();
            }

            slideTimer.Start();
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            int radius = 20;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();

            // Only round top-right and bottom-right corners
            path.AddLine(0, 0, panel1.Width - radius, 0);                    // top edge
            path.AddArc(panel1.Width - radius, 0, radius, radius, 270, 90); // top-right
            path.AddLine(panel1.Width, radius, panel1.Width, panel1.Height - radius); // right edge
            path.AddArc(panel1.Width - radius, panel1.Height - radius, radius, radius, 0, 90); // bottom-right
            path.AddLine(panel1.Width - radius, panel1.Height, 0, panel1.Height);             // bottom edge
            path.AddLine(0, panel1.Height, 0, 0);                                           // left edge
            path.CloseFigure();

            panel1.Region = new Region(path);
        }
    }
}
