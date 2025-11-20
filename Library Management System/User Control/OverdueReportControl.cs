using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Helpers;

namespace Library_Management_System.User_Control
{

    public partial class OverdueReportControl : UserControl
    {
        private MainForm mainForm;
        private int targetWidth = 150; // final width when fully shown
        private Timer slideTimer;

        public OverdueReportControl(MainForm parentForm)
        {
            InitializeComponent();
            // Initially collapsed
            panel1.Width = 0;

            // Setup Timer
            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;

            mainForm = parentForm;
            SetupDataGridView();
            this.Load += OverdueReportControl_Load;
        }
        // ✅ Default constructor (for designer or other forms)
        public OverdueReportControl()
        {
            InitializeComponent();
            SetupDataGridView();
            this.Load += OverdueReportControl_Load;
        }


        private void OverdueReportControl_Load(object sender, EventArgs e)
        {
            Penalties.SyncPenaltiesFromBorrowings();
            LoadOverdueBooks();
        }

        private void SetupDataGridView()
        {
            dgvBorrowedBooks.Columns.Clear();
            dgvBorrowedBooks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBorrowedBooks.AllowUserToAddRows = false;
            dgvBorrowedBooks.ReadOnly = true;
            dgvBorrowedBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBorrowedBooks.RowHeadersVisible = false;
            dgvBorrowedBooks.BackgroundColor = Color.White;

            // Add numbering column
            dgvBorrowedBooks.Columns.Add("No", "No.");
            dgvBorrowedBooks.Columns["No"].Width = 50;
            dgvBorrowedBooks.Columns["No"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvBorrowedBooks.Columns.Add("StudentNo", "Student No");
            dgvBorrowedBooks.Columns.Add("MemberName", "Student Name");
            dgvBorrowedBooks.Columns.Add("BookTitle", "Book Title");
            dgvBorrowedBooks.Columns.Add("BorrowDate", "Borrow Date");
            dgvBorrowedBooks.Columns.Add("DueDate", "Due Date");
            dgvBorrowedBooks.Columns.Add("ReturnDate", "Return Date");

            // ✅ Single column for overdue duration
            dgvBorrowedBooks.Columns.Add("Overdue", "Overdue");

            dgvBorrowedBooks.Columns.Add("Penalty", "Penalty (₱)");
            dgvBorrowedBooks.Columns.Add("Status", "Status");

            // Styling
            dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvBorrowedBooks.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvBorrowedBooks.RowTemplate.Height = 40;
            dgvBorrowedBooks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
        }


        private void LoadOverdueBooks()
        {
            dgvBorrowedBooks.Rows.Clear();
            int counter = 1;

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Sync penalties first
                Penalties.SyncPenaltiesFromBorrowings();

                DateTime now = DateTime.Now;

                string query = @"
                SELECT 
                    m.StudentNo,
                    (m.FirstName || ' ' || m.LastName) AS MemberName,
                    bks.Title AS BookTitle,
                    br.BorrowDate,
                    br.DueDate,
                    br.ReturnDate,
                    br.Status,
                    p.Amount AS PenaltyFromTable,
                    p.DaysOverdue AS DaysOverdueFromTable,
                    p.HoursOverdue AS HoursOverdueFromTable
                FROM Borrowings br
                INNER JOIN Members m ON br.MemberId = m.MemberId
                INNER JOIN Books bks ON br.BookId = bks.BookId
                LEFT JOIN Penalties p ON br.BorrowId = p.BorrowId
                WHERE (br.Status = 'Returned' AND br.ReturnDate > br.DueDate)
                   OR (br.Status = 'Borrowed' AND br.ReturnDate IS NULL AND datetime('now', 'localtime') > br.DueDate)
                ORDER BY br.DueDate DESC;
            ";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@now", now);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime borrowDate = Convert.ToDateTime(reader["BorrowDate"]);
                            DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                            DateTime? returnDate = reader["ReturnDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReturnDate"]);

                            // Calculate penalty dynamically
                            int daysOverdue, hoursOverdue;
                            double penalty;
                            PenaltyHelper.CalculatePenalty(dueDate, returnDate, out penalty, out daysOverdue, out hoursOverdue);

                            // Override with stored values if available
                            if (reader["PenaltyFromTable"] != DBNull.Value)
                                penalty = Convert.ToDouble(reader["PenaltyFromTable"]);
                            if (reader["DaysOverdueFromTable"] != DBNull.Value)
                                daysOverdue = Convert.ToInt32(reader["DaysOverdueFromTable"]);
                            if (reader["HoursOverdueFromTable"] != DBNull.Value)
                                hoursOverdue = Convert.ToInt32(reader["HoursOverdueFromTable"]);

                            // Create readable overdue text
                            string overdueText = daysOverdue > 0
                                ? daysOverdue == 1 ? "1 day" : $"{daysOverdue} days"
                                : hoursOverdue > 0
                                    ? hoursOverdue == 1 ? "1 hour" : $"{hoursOverdue} hours"
                                    : "-";

                            // Determine status text
                            string status = returnDate.HasValue && returnDate.Value > dueDate ? "Returned Late" : "Borrowed";

                            // Add row to DataGridView with date + time
                            int rowIndex = dgvBorrowedBooks.Rows.Add(
                                counter++,
                                reader["StudentNo"],
                                reader["MemberName"],
                                reader["BookTitle"],
                                borrowDate.ToString("yyyy-MM-dd HH:mm"),
                                dueDate.ToString("yyyy-MM-dd HH:mm"),
                                returnDate.HasValue ? returnDate.Value.ToString("yyyy-MM-dd HH:mm") : "Not Returned",
                                overdueText,
                                penalty.ToString("₱0.00"),
                                status
                            );

                            // Set Status color
                            dgvBorrowedBooks.Rows[rowIndex].Cells["Status"].Style.ForeColor =
                                status == "Returned Late" ? Color.Green : Color.Red;
                        }
                    }
                }
            }

            // Clear selection
            if (dgvBorrowedBooks.Rows.Count > 0)
            {
                dgvBorrowedBooks.ClearSelection();
                dgvBorrowedBooks.CurrentCell = null;
            }

            // ✅ Refresh dashboard via MainForm reference
            mainForm?.DashboardInstance?.RefreshPenaltySummary();

            if (dgvBorrowedBooks.Rows.Count == 0)
                MessageBox.Show("No overdue books found.", "Overdue Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional event for clicks
        }

        private void OverdueReportControl_Load_1(object sender, EventArgs e)
        {

        }

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
