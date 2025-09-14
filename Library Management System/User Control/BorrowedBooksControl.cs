using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control
{
    public partial class BorrowBooksControl : UserControl
    {
        private const int MaxBorrowLimit = 1;
        private const int DefaultLoanDays = 1; // due date = borrowdate + 1 day

        private int currentMemberId = -1;

        public BorrowBooksControl()
        {
            InitializeComponent();
            LoadAvailableBooks();
        }

        private void LoadAvailableBooks()
        {
            var dt = DatabaseHelper.Query("SELECT BookID, Title, Author, AvailableCopies FROM Books WHERE AvailableCopies > 0");
            dgvAvailableBooks.DataSource = dt;
        }

        private void BtnLoadMember_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";
            if (!int.TryParse(txtMemberID.Text.Trim(), out int memberId))
            {
                lblMessage.Text = "Enter valid Member ID.";
                return;
            }

            // Get member details
            var dt = DatabaseHelper.Query(@"
                SELECT 
                    MemberID, 
                    (FirstName || ' ' || LastName) AS FullName, 
                    IsActive 
                FROM Members 
                WHERE MemberID = @m",
                new SQLiteParameter("@m", memberId));

            if (dt.Rows.Count == 0)
            {
                lblMessage.Text = "Member not found.";
                currentMemberId = -1;
                lblMemberName.Text = "";
                return;
            }

            var row = dt.Rows[0];
            bool isActive = Convert.ToInt32(row["IsActive"]) == 1;
            if (!isActive)
            {
                lblMessage.Text = "Member is inactive.";
                currentMemberId = -1;
                lblMemberName.Text = row["FullName"].ToString();
                return;
            }

            currentMemberId = memberId;
            lblMemberName.Text = row["FullName"].ToString();

            // Show due date
            lblDueDate.Text = "Due Date: " + DateTime.Now.AddDays(DefaultLoanDays).ToShortDateString();

            // Current borrowed count
            var dtCount = DatabaseHelper.Query(
                "SELECT COUNT(*) as cnt FROM Borrowing WHERE MemberID = @m AND ReturnDate IS NULL",
                new SQLiteParameter("@m", currentMemberId));

            int borrowedCount = Convert.ToInt32(dtCount.Rows[0]["cnt"]);
            lblMessage.Text = $"Current borrowed: {borrowedCount}. Max allowed: {MaxBorrowLimit}";
        }

        private void BtnIssue_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            if (currentMemberId <= 0)
            {
                lblMessage.Text = "Load a valid member first.";
                return;
            }

            if (dgvAvailableBooks.SelectedRows.Count == 0)
            {
                lblMessage.Text = "Select a book to issue.";
                return;
            }

            // Check borrow limit
            var dtCount = DatabaseHelper.Query(
                "SELECT COUNT(*) as cnt FROM Borrowing WHERE MemberID = @m AND ReturnDate IS NULL",
                new SQLiteParameter("@m", currentMemberId));

            int borrowedCount = Convert.ToInt32(dtCount.Rows[0]["cnt"]);
            if (borrowedCount >= MaxBorrowLimit)
            {
                lblMessage.Text = $"Member reached borrow limit ({MaxBorrowLimit}).";
                return;
            }

            var sel = dgvAvailableBooks.SelectedRows[0];
            int bookId = Convert.ToInt32(sel.Cells["BookID"].Value);

            DateTime borrowDate = DateTime.Now.Date;
            DateTime dueDate = borrowDate.AddDays(DefaultLoanDays);

            using (var conn = new SQLiteConnection("Data Source=library.db;Version=3;"))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    // Insert borrowing
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "INSERT INTO Borrowing (MemberID, BookID, BorrowDate, DueDate) VALUES (@m,@b,@bd,@dd)";
                        cmd.Parameters.AddWithValue("@m", currentMemberId);
                        cmd.Parameters.AddWithValue("@b", bookId);
                        cmd.Parameters.AddWithValue("@bd", borrowDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@dd", dueDate.ToString("yyyy-MM-dd"));
                        cmd.ExecuteNonQuery();
                    }

                    // Decrement available copies
                    using (var cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookID = @b";
                        cmd2.Parameters.AddWithValue("@b", bookId);
                        cmd2.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
            }

            lblMessage.Text = "Book issued successfully.";
            LoadAvailableBooks();

            // ✅ Refresh dashboard counts instantly
            MainForm parentForm = this.FindForm() as MainForm;
            if (parentForm != null)
            {
                foreach (Control ctrl in parentForm.Controls)
                {
                    if (ctrl is DashboardControl dashboard)
                    {
                        dashboard.RefreshStats();
                        break;
                    }
                }
            }
        }

        private void BorrowBooksControl_Load(object sender, EventArgs e)
        {

        }

        private void dgvAvailableBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
