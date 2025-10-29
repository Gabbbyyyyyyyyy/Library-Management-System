using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Helpers;

namespace Library_Management_System.User_Control
{
    public partial class OverdueReportControl : UserControl
    {
        public OverdueReportControl()
        {
            InitializeComponent();
            SetupDataGridView();
            this.Load += OverdueReportControl_Load;
        }

        private void OverdueReportControl_Load(object sender, EventArgs e)
        {
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
            dgvBorrowedBooks.Columns.Add("No", "#");
            dgvBorrowedBooks.Columns["No"].Width = 50;
            dgvBorrowedBooks.Columns["No"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvBorrowedBooks.Columns.Add("StudentNo", "Student No");
            dgvBorrowedBooks.Columns.Add("MemberName", "Member Name");
            dgvBorrowedBooks.Columns.Add("BookTitle", "Book Title");
            dgvBorrowedBooks.Columns.Add("BorrowDate", "Borrow Date");
            dgvBorrowedBooks.Columns.Add("DueDate", "Due Date");
            dgvBorrowedBooks.Columns.Add("ReturnDate", "Return Date");
            dgvBorrowedBooks.Columns.Add("DaysOverdue", "Days Overdue");
            dgvBorrowedBooks.Columns.Add("Penalty", "Penalty (₱)");
            dgvBorrowedBooks.Columns.Add("Status", "Status");

            // Styling
            dgvBorrowedBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvBorrowedBooks.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvBorrowedBooks.RowTemplate.Height = 40; // match dgvBooks row height
            dgvBorrowedBooks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
        }


        private void LoadOverdueBooks()
        {
            dgvBorrowedBooks.Rows.Clear();
            int counter = 1; // numbering starts at 1

            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                    SELECT 
                        m.StudentNo,
                        (m.FirstName || ' ' || m.LastName) AS MemberName,
                        bks.Title AS BookTitle,
                        br.BorrowDate,
                        br.DueDate,
                        br.ReturnDate,
                        br.Penalty,
                        br.Status,
                        p.Amount AS PenaltyFromTable,
                        p.DaysOverdue AS DaysOverdueFromTable
                    FROM Borrowings br
                    INNER JOIN Members m ON br.MemberId = m.MemberId
                    INNER JOIN Books bks ON br.BookId = bks.BookId
                    LEFT JOIN Penalties p ON br.BorrowId = p.BorrowId
                    WHERE br.Status = 'Returned' OR (br.ReturnDate IS NULL AND DATE('now') > br.DueDate)
                    ORDER BY br.DueDate DESC;
                ";

                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                        DateTime? returnDate = reader["ReturnDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ReturnDate"]);

                        // Use saved penalty from Penalties table or calculate dynamically
                        double penalty = reader["PenaltyFromTable"] != DBNull.Value ? Convert.ToDouble(reader["PenaltyFromTable"]) : 0;
                        int daysOverdue = reader["DaysOverdueFromTable"] != DBNull.Value ? Convert.ToInt32(reader["DaysOverdueFromTable"]) : 0;

                        if (penalty == 0) // dynamically calculate if no record exists
                        {
                            PenaltyHelper.CalculatePenalty(dueDate, returnDate, out penalty, out daysOverdue);
                        }

                        dgvBorrowedBooks.Rows.Add(
                            counter++, // numbering column
                            reader["StudentNo"],
                            reader["MemberName"],
                            reader["BookTitle"],
                            Convert.ToDateTime(reader["BorrowDate"]).ToString("yyyy-MM-dd"),
                            dueDate.ToString("yyyy-MM-dd"),
                            returnDate.HasValue ? returnDate.Value.ToString("yyyy-MM-dd") : "Not Returned",
                            daysOverdue,
                            penalty.ToString("₱0.00"),
                            reader["Status"]
                        );
                    }
                }
            }

            // Remove default selection
            if (dgvBorrowedBooks.Rows.Count > 0)
            {
                dgvBorrowedBooks.ClearSelection();
                dgvBorrowedBooks.CurrentCell = null; // optional: ensures no cell is focused
            }


            if (dgvBorrowedBooks.Rows.Count == 0)
                MessageBox.Show("No overdue books found.", "Overdue Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional event for clicks
        }
    }
}
