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
        private Panel noResultPanel;



        public ReturnBooksControl()
        {
            InitializeComponent();

            dgvBorrowedBooks.DataError += (s, e) =>
            {
                e.ThrowException = false; // prevents crash
            };

            btnReturn.ApplyRoundedCorners(10, Color.FromArgb(211, 181, 139));
            // Initially collapsed
            panel1.Width = 0;

            // Setup Timer
            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;
            // Apply default DataGridView styling
            DataGridViewHelper.ApplyDefaultStyle(dgvBorrowedBooks);
            dgvBorrowedBooks.RowTemplate.Height = 40; // optional: keep taller rows

            dgvBorrowedBooks.CellFormatting += (s, e) =>
            {
                if (dgvBorrowedBooks.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                {
                    string status = e.Value.ToString().Trim();

                    switch (status)
                    {
                        case "Pending":
                            e.CellStyle.ForeColor = Color.Orange;
                            break;
                        case "Returned":
                            e.CellStyle.ForeColor = Color.Green;
                            break;
                        case "Accepted":
                            e.CellStyle.ForeColor = Color.Blue;
                            break;
                        default:
                            e.CellStyle.ForeColor = Color.Black;
                            break;
                    }

                    e.FormattingApplied = true;
                }

                // Optional: format Penalty column
                if (dgvBorrowedBooks.Columns[e.ColumnIndex].Name == "Penalty" && e.Value != null)
                {
                    e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    e.CellStyle.Format = "₱0.00";
                    e.FormattingApplied = true;
                }
            };

            // Hook up search
            txtSearch.KeyDown += TxtSearchReturn_KeyDown;
            txtSearch.TextChanged += TxtSearchReturn_TextChanged;

             // Create hidden no-result panel
            noResultPanel = new Panel
            {
                Size = dgvBorrowedBooks.Size,
                Location = dgvBorrowedBooks.Location,
                BackColor = Color.Transparent,
                Visible = false
            };
            this.Controls.Add(noResultPanel);

            // Add the picture
            PictureBox pic = new PictureBox
            {
                Image = Properties.Resources.NoBooksIcon,   // <-- Put your icon in Resources
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            noResultPanel.Controls.Add(pic);

            // Center later in RefreshNoResultLayout()

            // Main title
            Label lblMain = new Label
            {
                Text = "No returns found",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Gray,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            noResultPanel.Controls.Add(lblMain);

            // Subtitle
            Label lblSub = new Label
            {
                Text = "Try searching again or wait for a return.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.DimGray,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            noResultPanel.Controls.Add(lblSub);

            // Save controls for resizing later
            noResultPanel.Tag = new Control[] { pic, lblMain, lblSub };



            // Placeholder text settings
            txtSearch.ForeColor = Color.Gray;
            txtSearch.Text = "Search name or book...";

            txtSearch.Enter += (s, e) =>
            {
                if (txtSearch.Text == "Search name or book...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };

            txtSearch.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Search name or book...";
                    txtSearch.ForeColor = Color.Gray;
                }
            };


            LoadBorrowedBooks();

            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            Library_Management_System.User_Control_Student.Borrowing.BookReturned += LoadBorrowedBooks;
        }

        private void RefreshNoResultLayout(string mainText, string subText)
        {
            var controls = (Control[])noResultPanel.Tag;
            PictureBox pic = (PictureBox)controls[0];
            Label lblMain = (Label)controls[1];
            Label lblSub = (Label)controls[2];

            lblMain.Text = mainText;
            lblSub.Text = subText;

            // Center icon
            pic.Location = new Point(
                (noResultPanel.Width - pic.Width) / 2,
                20
            );

            // Center text
            lblMain.Location = new Point(
                (noResultPanel.Width - lblMain.Width) / 2,
                pic.Bottom + 15
            );

            lblSub.Location = new Point(
                (noResultPanel.Width - lblSub.Width) / 2,
                lblMain.Bottom + 5
            );
        }


        private void TxtSearchReturn_TextChanged(object sender, EventArgs e)
        {
            PerformSearchReturn();
        }

        private void TxtSearchReturn_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // prevent beep
                PerformSearchReturn();
            }
        }

        private void PerformSearchReturn()
        {
            string searchText = txtSearch.Text.Trim();

            // 🔥 FIX: If placeholder is active, DO NOT run a search
            if (searchText == "Search name or book..." || string.IsNullOrWhiteSpace(searchText))
            {
                LoadBorrowedBooks();
                return;
            }


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
          AND (m.FirstName LIKE @search OR m.LastName LIKE @search OR b.Title LIKE @search)
        ORDER BY br.DueDate ASC;
    ", new SQLiteParameter("@search", "%" + searchText + "%"));

            // Recalculate penalty for filtered rows
            foreach (DataRow row in dt.Rows)
            {
                if (row["DueDate"] != DBNull.Value)
                {
                    DateTime dueDate = Convert.ToDateTime(row["DueDate"]);
                    DateTime? returnDate = row["ReturnDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["ReturnDate"]);

                    double totalPenalty;
                    int daysOverdue, hoursOverdue;
                    PenaltyHelper.CalculatePenalty(dueDate, returnDate, out totalPenalty, out daysOverdue, out hoursOverdue);
                    row["Penalty"] = totalPenalty;
                }
            }

            dgvBorrowedBooks.DataSource = dt;

            // Optional: adjust headers again
            dgvBorrowedBooks.Columns["FullName"].HeaderText = "Name of Students";
            dgvBorrowedBooks.Columns["BorrowDate"].HeaderText = "Borrow Date";
            dgvBorrowedBooks.Columns["ReturnDate"].HeaderText = "Return Date";
            dgvBorrowedBooks.Columns["DueDate"].HeaderText = "Due Date";
            if (dgvBorrowedBooks.Columns.Contains("BorrowId"))
                dgvBorrowedBooks.Columns["BorrowId"].Visible = false;
            // Fix Penalty column type
            if (dt.Columns.Contains("Penalty"))
            {
                dt.Columns["Penalty"].ReadOnly = false;
                dt.Columns["Penalty"].DataType = typeof(double);

                foreach (DataRow row in dt.Rows)
                {
                    if (row["Penalty"] == DBNull.Value || row["Penalty"].ToString() == "")
                        row["Penalty"] = 0.0;
                }
            }
            // ----- NO DATA OR NO SEARCH RESULT -----
            if (dt.Rows.Count == 0)
            {
                noResultPanel.Visible = true;
                dgvBorrowedBooks.Visible = false;

                if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search name or book...")
                    RefreshNoResultLayout("No return records yet", "Returned books will appear here.");
                else
                    RefreshNoResultLayout("No match found", "Try checking the spelling or use different keywords.");

                // Disable search only if no returns at all
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search name or book...")
                    txtSearch.Enabled = false;
            }
            else
            {
                dgvBorrowedBooks.Visible = true;
                noResultPanel.Visible = false;

                txtSearch.Enabled = true;
            }
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


            // Fix Penalty column type
            if (dt.Columns.Contains("Penalty"))
            {
                dt.Columns["Penalty"].ReadOnly = false;
                dt.Columns["Penalty"].DataType = typeof(double);

                foreach (DataRow row in dt.Rows)
                {
                    if (row["Penalty"] == DBNull.Value || row["Penalty"].ToString() == "")
                        row["Penalty"] = 0.0;
                }
            }
            if (dt.Rows.Count == 0)
            {
                dgvBorrowedBooks.Visible = false;
                noResultPanel.Visible = true;
                RefreshNoResultLayout("No return records yet", "Returned books will appear here.");

                // Disable search
                txtSearch.Enabled = false;
            }
            else
            {
                dgvBorrowedBooks.Visible = true;
                noResultPanel.Visible = false;

                // Enable search
                txtSearch.Enabled = true;
            }

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