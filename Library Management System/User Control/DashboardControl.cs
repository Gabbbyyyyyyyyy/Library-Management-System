using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control
{
    public partial class DashboardControl : UserControl
    {
        private Timer _greetingTimer;
        private string _userDisplayName;

        public DashboardControl(string userDisplayName)
        {
            InitializeComponent();
            _userDisplayName = string.IsNullOrWhiteSpace(userDisplayName) ? "ADMIN" : userDisplayName;

            // Initialize greeting
            UpdateGreeting();

            // Timer to update greeting every minute
            _greetingTimer = new Timer();
            _greetingTimer.Interval = 60 * 1000;
            _greetingTimer.Tick += (s, e) => UpdateGreeting();
            _greetingTimer.Start();

            // Load dashboard stats
            LoadDashboardStats();

            // Wire label clicks to panel clicks
            lblTotalBooks.Click += pnlTotalBooks_Click;
            lblActiveMembers.Click += pnlActiveMembers_Click;
        }

        // Parameterless constructor defaults to ADMIN
        public DashboardControl() : this("ADMIN") { }

        private void UpdateGreeting()
        {
            string period = GetTimePeriod(DateTime.Now);
            lblGreeting.Text = $"Good {period}, {_userDisplayName.ToUpper()}!";
        }

        private string GetTimePeriod(DateTime dt)
        {
            int hour = dt.Hour;
            if (hour >= 5 && hour < 12) return "morning";
            else if (hour >= 12 && hour < 17) return "afternoon";
            else if (hour >= 17 && hour < 21) return "evening";
            else return "night";
        }

        private void LoadDashboardStats()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                int totalBooks = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Books", con).ExecuteScalar());
                int borrowedBooks = Convert.ToInt32(new SQLiteCommand(
                    "SELECT COUNT(*) FROM BorrowedBooks WHERE ReturnDate IS NULL", con).ExecuteScalar());
                int availableBooks = totalBooks - borrowedBooks;
                int activeMembers = Convert.ToInt32(new SQLiteCommand(
                    "SELECT COUNT(*) FROM Members WHERE IsActive = 1", con).ExecuteScalar());
                int overdueBooks = Convert.ToInt32(new SQLiteCommand(
                    "SELECT COUNT(*) FROM BorrowedBooks WHERE ReturnDate IS NULL AND DueDate < DATE('now')", con).ExecuteScalar());

                lblTotalBooks.Text = $"📚 Total Books: {totalBooks}";
                lblBorrowedBooks.Text = $"📖 Borrowed: {borrowedBooks}";
                lblAvailableBooks.Text = $"✅ Available: {availableBooks}";
                lblActiveMembers.Text = $"👥 Active Members: {activeMembers}";
                lblOverdueBooks.Text = $"⚠️ Overdue: {overdueBooks}";
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_greetingTimer != null)
            {
                _greetingTimer.Stop();
                _greetingTimer.Dispose();
            }
            base.OnHandleDestroyed(e);
        }

        // ------------------- Navigation -------------------

        // Panel & label click for Total Books
        private void pnlTotalBooks_Click(object sender, EventArgs e)
        {
            OpenManageBooksControl();
        }

        private void OpenManageBooksControl()
        {
            MainForm parentForm = this.FindForm() as MainForm;
            if (parentForm != null)
            {
                parentForm.LoadControl(new ManageBooksControl());
            }
        }

        // Panel & label click for Active Members
        private void pnlActiveMembers_Click(object sender, EventArgs e)
        {
            OpenManageMembersControl();
        }

        private void OpenManageMembersControl()
        {
            MainForm parentForm = this.FindForm() as MainForm;
            if (parentForm != null)
            {
                parentForm.LoadControl(new ManageMembersControl());
            }
        }

        // Optional: Replace other panels with inline navigation if needed
        private void pnlBorrowedBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            if (parentForm != null)
            {
                parentForm.LoadControl(new BorrowedBooksControl());
            }
        }

        private void pnlAvailableBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            if (parentForm != null)
            {
                parentForm.LoadControl(new BooksForm());
            }
        }

        private void pnlOverdueBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            if (parentForm != null)
            {
                parentForm.LoadControl(new OverdueReportControl());
            }
        }
    }
}
