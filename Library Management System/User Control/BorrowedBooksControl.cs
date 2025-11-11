using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Library_Management_System.Models;
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

            // ✅ Ensure Borrowings table has Status column
            EnsureBorrowingsStatusColumn();
            EnsureBooksStatusColumn();

            dgvAvailableBooks.CellFormatting += dgvAvailableBooks_CellFormatting;
            dgvAvailableBooks.EnableHeadersVisualStyles = false;
            dgvAvailableBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvAvailableBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.White;


            dgvAvailableBooks.DataBindingComplete += (s, e) =>
            {
                dgvAvailableBooks.ClearSelection();
                dgvAvailableBooks.CurrentCell = null;
            };

            // Set search box style
            
            txtSearch.Font = new Font(txtSearch.Font.FontFamily, 10, txtSearch.Font.Style);

            LoadAvailableBooks();
            SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 0, "Search books...");
            txtSearch.ForeColor = Color.Black;
            txtSearch.KeyDown += txtSearch_KeyDown;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private void dgvAvailableBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvAvailableBooks.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString().Trim();

                switch (status)
                {
                    case "Available":
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                    case "Not Available":
                        e.CellStyle.ForeColor = Color.Red;
                        break;
                    case "Reserved":
                        e.CellStyle.ForeColor = Color.Orange;
                        break;
                    default:
                        e.CellStyle.ForeColor = Color.Black;
                        break;
                }

                e.FormattingApplied = true;
            }
        }


        // ✅ Automatically check and add missing Status column
        private void EnsureBorrowingsStatusColumn()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                bool hasStatusColumn = false;

                using (var cmd = new SQLiteCommand("PRAGMA table_info(Borrowings);", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader["name"].ToString();
                        if (columnName.Equals("Status", StringComparison.OrdinalIgnoreCase))
                        {
                            hasStatusColumn = true;
                            break;
                        }
                    }
                }

                if (!hasStatusColumn)
                {
                    using (var alterCmd = new SQLiteCommand("ALTER TABLE Borrowings ADD COLUMN Status TEXT DEFAULT 'Borrowed';", con))
                    {
                        alterCmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("✅ Added missing 'Status' column to Borrowings table.");
                }
            }
        }

        private void LoadAvailableBooks()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                   SELECT 
                     b.BookId, b.ISBN, b.Title, b.Author, b.Category, 
                     b.Quantity, b.AvailableCopies,
                     CASE WHEN b.AvailableCopies = 0 THEN 'Not Available' ELSE 'Available' END AS Status
                    FROM Books b;
";

                using (var cmd = new SQLiteCommand(query, con))
                using (var da = new SQLiteDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvAvailableBooks.DataSource = dt;

                    dgvAvailableBooks.ReadOnly = true;
                    dgvAvailableBooks.RowTemplate.Height = 40;
                    dgvAvailableBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    dgvAvailableBooks.AllowUserToAddRows = false;

                    // ✅ Forcefully clear any lingering selection and focus
                    dgvAvailableBooks.ClearSelection();
                    dgvAvailableBooks.CurrentCell = null;
                    dgvAvailableBooks.Focus(); // optional: removes dotted focus box




                    //ColorStatusColumnText();
                }
            }
        }
        private void EnsureBooksStatusColumn()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                bool hasStatusColumn = false;

                using (var cmd = new SQLiteCommand("PRAGMA table_info(Books);", con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader["name"].ToString();
                        if (columnName.Equals("Status", StringComparison.OrdinalIgnoreCase))
                        {
                            hasStatusColumn = true;
                            break;
                        }
                    }
                }

                if (!hasStatusColumn)
                {
                    using (var alterCmd = new SQLiteCommand(
                        "ALTER TABLE Books ADD COLUMN Status TEXT DEFAULT 'Available';", con))
                    {
                        alterCmd.ExecuteNonQuery();
                    }

                    using (var updateCmd = new SQLiteCommand(
                        "UPDATE Books SET Status = CASE WHEN AvailableCopies > 0 THEN 'Available' ELSE 'Not Available' END;", con))
                    {
                        updateCmd.ExecuteNonQuery();
                    }

                    Console.WriteLine("✅ Added missing 'Status' column to Books table.");
                }
            }
        }





        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"
                    SELECT 
                        b.BookId, b.ISBN, b.Title, b.Author, b.Category, 
                        b.Quantity, b.AvailableCopies,
                        CASE 
                            WHEN b.AvailableCopies = 0 THEN 'Not Available' 
                            ELSE 'Available' 
                        END AS Status
                    FROM Books b
                    WHERE b.Title LIKE @search 
                       OR b.Author LIKE @search 
                       OR b.Category LIKE @search 
                       OR b.ISBN LIKE @search";


                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + txtSearch.Text + "%");
                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvAvailableBooks.DataSource = dt;
                        dgvAvailableBooks.ClearSelection();
                        dgvAvailableBooks.CurrentCell = null;


                        //ColorStatusColumnText();
                    }
                }
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            string searchText = txtSearch.Text.Trim();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"
                    SELECT 
                        b.BookId, b.ISBN, b.Title, b.Author, b.Category, 
                        b.Quantity, b.AvailableCopies,
                        CASE 
                            WHEN b.AvailableCopies = 0 THEN 'Not Available' 
                            ELSE 'Available' 
                        END AS Status
                    FROM Books b
                    WHERE b.Title LIKE @search 
                       OR b.Author LIKE @search 
                       OR b.Category LIKE @search 
                       OR b.ISBN LIKE @search";


                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvAvailableBooks.DataSource = dt;
                        dgvAvailableBooks.ClearSelection();
                        dgvAvailableBooks.CurrentCell = null;


                    }
                }
            }
        }



        private void BtnLoadMember_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "";

            string studentNo = txtMemberID.Text.Trim();
            if (string.IsNullOrEmpty(studentNo))
            {
                lblMessage.Text = "Enter a Student Number.";
                return;
            }

            DataTable dtMember = new DataTable();

            using (var con = Db.GetConnection())
            {
                con.Open();

                // 🔍 Actual search query
                string sql = @"
            SELECT 
                MemberId,
                StudentNo,
                (FirstName || ' ' || LastName) AS FullName,
                IsActive
            FROM Members
            WHERE TRIM(CAST(StudentNo AS TEXT)) = TRIM(CAST(@s AS TEXT));";

                using (var cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@s", studentNo);

                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dtMember);
                    }
                }
            }

            // 🧠 Now process results outside the connection
            if (dtMember.Rows.Count == 0)
            {
                lblMessage.Text = $"No member found with Student Number: {studentNo}";
                currentMemberId = -1;
                lblMemberName.Text = "";
                return;
            }

            var row = dtMember.Rows[0];

            // ⚙️ Check if member is active
            bool isActive = row["IsActive"] != DBNull.Value && Convert.ToInt32(row["IsActive"]) == 1;
            if (!isActive)
            {
                lblMessage.Text = "Member is inactive.";
                currentMemberId = -1;
                lblMemberName.Text = row["FullName"].ToString();
                return;
            }

            // ✅ Store MemberId internally
            currentMemberId = Convert.ToInt32(row["MemberId"]);

            // 🧾 Display member info
            lblMemberName.Text = row["FullName"].ToString();

            // Set due date to tomorrow 8 AM
            DateTime dueDate = DateTime.Now.Date.AddDays(DefaultLoanDays).AddHours(8);
            lblDueDate.Text = "Due Date: " + dueDate.ToString("yyyy-MM-dd HH:mm tt");

            // 📚 Count currently borrowed books
            var dtCount = DatabaseHelper.Query(
                "SELECT COUNT(*) AS cnt FROM Borrowings WHERE MemberID = @m AND ReturnDate IS NULL",
                new SQLiteParameter("@m", currentMemberId));

            int borrowedCount = Convert.ToInt32(dtCount.Rows[0]["cnt"]);
            lblMessage.Text = $"Student {studentNo} currently borrowed {borrowedCount}. Max allowed: {MaxBorrowLimit}";
        }







        private void BtnIssue_Click(object sender, EventArgs e)
        {

            if (!LibraryStatusHelper.IsLibraryOpen())
            {
                MessageBox.Show("The library is currently closed. Transactions are paused.",
                                "Library Closed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


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

            var dtCount = DatabaseHelper.Query(
                "SELECT COUNT(*) as cnt FROM Borrowings WHERE MemberID = @m AND ReturnDate IS NULL",
                new SQLiteParameter("@m", currentMemberId));

            int borrowedCount = Convert.ToInt32(dtCount.Rows[0]["cnt"]);
            if (borrowedCount >= MaxBorrowLimit)
            {
                lblMessage.Text = $"Member reached borrow limit ({MaxBorrowLimit}).";
                return;
            }

            var sel = dgvAvailableBooks.SelectedRows[0];
            int bookId = Convert.ToInt32(sel.Cells["BookID"].Value);

            // Check available copies
            int availableCopies = Convert.ToInt32(sel.Cells["AvailableCopies"].Value);
            if (availableCopies <= 0)
            {
                lblMessage.Text = "Cannot issue this book. No copies available.";
                return;
            }
            // Use full timestamp for BorrowDate, optional short date for DueDate
            DateTime borrowDate = DateTime.Now;
            // Set due date to tomorrow 8:00 AM
            DateTime dueDate = borrowDate.Date.AddDays(1).AddHours(8);

            using (var conn = Db.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "INSERT INTO Borrowings (MemberID, BookID, BorrowDate, DueDate, Status) VALUES (@m,@b,@bd,@dd,'Borrowed')";
                        cmd.Parameters.AddWithValue("@m", currentMemberId);
                        cmd.Parameters.AddWithValue("@b", bookId);
                        cmd.Parameters.AddWithValue("@bd", borrowDate.ToString("yyyy-MM-dd HH:mm:ss")); // include time
                        cmd.Parameters.AddWithValue("@dd", dueDate.ToString("yyyy-MM-dd HH:mm:ss"));     // include time
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd2 = new SQLiteCommand(conn))
                    {
                        cmd2.CommandText = "UPDATE Books\r\nSET AvailableCopies = CASE WHEN AvailableCopies > 0 THEN AvailableCopies - 1 ELSE 0 END\r\nWHERE BookID = @b;\r\n";
                        cmd2.Parameters.AddWithValue("@b", bookId);
                        cmd2.ExecuteNonQuery();
                    }
                    using (var cmd3 = new SQLiteCommand(conn))
                    {
                        cmd3.CommandText = "UPDATE Books SET Status = CASE WHEN AvailableCopies > 0 THEN 'Available' ELSE 'Not Available' END WHERE BookID = @b";
                        cmd3.Parameters.AddWithValue("@b", bookId);
                        cmd3.ExecuteNonQuery();
                    }


                    trans.Commit();
                }
            }

            lblMessage.Text = "Book issued successfully.";


            // Refresh available books and reapply status colors
            LoadAvailableBooks();

            // ✅ Force clear lingering selection/focus
            dgvAvailableBooks.ClearSelection();
            dgvAvailableBooks.CurrentCell = null;
            dgvAvailableBooks.Focus();
            //ColorStatusColumnText();

            if (this.FindForm() is MainForm parentForm)
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


        private void BorrowBooksControl_Load(object sender, EventArgs e) { }
        private void dgvAvailableBooks_CellContentClick(object sender, DataGridViewCellEventArgs e) {
        }



        private void txtMemberID_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
