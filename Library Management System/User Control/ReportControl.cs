using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control
{
    public partial class ReportControl : UserControl
    {
        // Controls
        private ComboBox cmbReportType, cmbStudent, cmbBook;
        private DateTimePicker dtpFrom, dtpTo;
        private Button btnApplyFilter, btnExport;
        private DataGridView dgvReports;

        public ReportControl()
        {
            Db.EnsureCreated();
            InitializeReportControls();  // ✅ Must be called here
            this.Load += ReportControl_Load;
            // Apply default DataGridView styling
            DataGridViewHelper.ApplyDefaultStyle(dgvReports);
            dgvReports.RowTemplate.Height = 40; // optional: keep taller rows
        }


        private void ReportControl_Load(object sender, EventArgs e)
        {
            LoadMembers();
            LoadBooks();
        }

        private void InitializeReportControls()
        {
            int startX = 220;
            int topMargin = 130;
            int padding = 10;

            Size comboSize = new Size(192, 33); // new size for all combo boxes and date pickers
            Font comboFont = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular);

            // Report Type ComboBox
            cmbReportType = new ComboBox
            {
                Location = new Point(startX, topMargin),
                Size = comboSize,              // apply size
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont               // apply font
            };
            cmbReportType.Items.AddRange(new string[]
            {
        "Dashboard",
        "Borrowing",
        "Returns",
        "Student Activity",
        "Books",
        "Students",
        //"Circulation",
        "Overdue",
        "Penalty/Fine"
            });
            cmbReportType.SelectedIndexChanged += CmbReportType_SelectedIndexChanged;
            cmbReportType.SelectedIndex = 0;
            this.Controls.Add(cmbReportType);

            // Date Filters
            dtpFrom = new DateTimePicker
            {
                Location = new Point(startX + comboSize.Width + padding, topMargin),
                Size = comboSize,
                Font = comboFont
            };
            dtpTo = new DateTimePicker
            {
                Location = new Point(startX + comboSize.Width * 2 + padding * 2, topMargin),
                Size = comboSize,
                Font = comboFont
            };
            this.Controls.Add(dtpFrom);
            this.Controls.Add(dtpTo);

            // Member & Book Filters
            cmbStudent = new ComboBox
            {
                Location = new Point(startX + comboSize.Width * 2 + comboSize.Width + padding * 3, topMargin),
                Size = comboSize,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont
            };
            cmbBook = new ComboBox
            {
                Location = new Point(startX + comboSize.Width * 3 + comboSize.Width + padding * 4, topMargin),
                Size = comboSize,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = comboFont
            };
            this.Controls.Add(cmbStudent);
            this.Controls.Add(cmbBook);

            // Buttons
            btnApplyFilter = new Button
            {
                Text = "Apply Filter",
                Location = new Point(cmbBook.Right + padding, topMargin), // right after cmbBook
                Size = comboSize,
                Font = comboFont
            };
            btnApplyFilter.Click += BtnApplyFilter_Click;
            this.Controls.Add(btnApplyFilter);

            // Example: make it wider
            // Export button as icon
            btnExport = new Button
            {
                Text = "", // no text
                Location = new Point(btnApplyFilter.Right + padding, topMargin), // right after Apply Filter
                Size = new Size(32, comboSize.Height), // icon size: 32px width, same height as combo box
                Font = comboFont,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.Transparent,
                BackgroundImage = Properties.Resources.export,
                BackgroundImageLayout = ImageLayout.Stretch
            };

            // Set icon
            btnExport.BackgroundImage = Properties.Resources.export;
            btnExport.BackgroundImageLayout = ImageLayout.Stretch;

            btnExport.Click += BtnExport_Click;
            this.Controls.Add(btnExport);

            // DataGridView
            dgvReports = new DataGridView
            {
                Location = new Point(209, 165),
                Size = new Size(1295, 710),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            this.Controls.Add(dgvReports);
        }

        private void CmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReportColumns(cmbReportType.SelectedItem.ToString());
        }

        private void LoadReportColumns(string reportType)
        {
            if (dgvReports == null) return; // prevents null reference
            dgvReports.Columns.Clear();
            switch (reportType)
            {
                case "Dashboard":
                    dgvReports.Columns.Add("TotalBooks", "Total Books");
                    dgvReports.Columns.Add("TotalMembers", "Total Students");
                    dgvReports.Columns.Add("BorrowedBooks", "Borrowed Books");
                    dgvReports.Columns.Add("OverdueBooks", "Overdue Books");
                    break;
                case "Borrowing":
                    dgvReports.Columns.Add("MemberName", "Students Name / ID");
                    dgvReports.Columns.Add("BookTitle", "Book Title / ID");
                    dgvReports.Columns.Add("BorrowDate", "Borrow Date");
                    dgvReports.Columns.Add("DueDate", "Due Date");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
                case "Returns":
                    dgvReports.Columns.Add("MemberName", "Students");
                    dgvReports.Columns.Add("BookTitle", "Book");
                    dgvReports.Columns.Add("BorrowDate", "Borrowed On");
                    dgvReports.Columns.Add("ReturnDate", "Returned On");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
                case "Student Activity":
                    dgvReports.Columns.Add("MemberName", "Students Name");
                    dgvReports.Columns.Add("BorrowedBooks", "Borrowed Books");
                    dgvReports.Columns.Add("ReturnedBooks", "Returned Books");
                    dgvReports.Columns.Add("PendingBorrowings", "Pending Borrowings");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
                case "Books":
                    //dgvReports.Columns.Add("BookId", "Book ID");
                    dgvReports.Columns.Add("Title", "Title");
                    dgvReports.Columns.Add("Author", "Author");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
                case "Students":
                    //dgvReports.Columns.Add("MemberId", "Student ID");
                    dgvReports.Columns.Add("FullName", "Full Name");
                    //dgvReports.Columns.Add("Email", "Email");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
                //case "Circulation":
                //    dgvReports.Columns.Add("MemberName", "Member");
                //    dgvReports.Columns.Add("BookTitle", "Book");
                //    dgvReports.Columns.Add("BorrowDate", "Borrow Date");
                //    dgvReports.Columns.Add("DueDate", "Due Date");
                //    dgvReports.Columns.Add("ReturnDate", "Return Date");
                //    dgvReports.Columns.Add("Status", "Status");
                //    break;
                case "Overdue":
                    dgvReports.Columns.Add("MemberName", "Students");
                    dgvReports.Columns.Add("BookTitle", "Book");
                    dgvReports.Columns.Add("DueDate", "Due Date");
                    dgvReports.Columns.Add("DaysOverdue", "Days Overdue");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
                case "Penalty/Fine":
                    dgvReports.Columns.Add("MemberName", "Students");
                    dgvReports.Columns.Add("BookTitle", "Book");
                    dgvReports.Columns.Add("Amount", "Amount");
                    dgvReports.Columns.Add("Status", "Status");
                    break;
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            string reportType = cmbReportType.SelectedItem.ToString();

            // First, define columns
            LoadReportColumns(reportType);

            // Then load data
            switch (reportType)
            {
                case "Dashboard":
                    LoadDashboardReport();
                    break;
                case "Borrowing":
                    LoadBorrowingReport();
                    break;
                case "Returns":
                    LoadReturnsReport();
                    break;
                case "Student Activity":
                    LoadStudentActivityReport();
                    break;
                case "Books":
                    LoadBooksReport();
                    break;
                case "Students":
                    LoadMembersReport();
                    break;
                //case "Circulation":
                //    LoadCirculationReport();
                    //break;
                case "Overdue":
                    LoadOverdueReport();
                    break;
                case "Penalty/Fine":
                    LoadPenaltyReport();
                    break;
            }
        }


        // ComboBox Loaders
        //private void LoadMembers() { cmbStudent.Items.Clear(); cmbStudent.Items.Add("All"); cmbStudent.SelectedIndex = 0; using (var con = Db.GetConnection()) { con.Open(); using (SQLiteCommand cmd = new SQLiteCommand("SELECT MemberId, FirstName || ' ' || LastName AS FullName FROM Members", con)) using (SQLiteDataReader reader = cmd.ExecuteReader()) { while (reader.Read()) { cmbStudent.Items.Add(new ComboBoxItem { Id = Convert.ToInt32(reader["MemberId"]), Name = reader["FullName"].ToString() }); } } } }
        // ComboBox Loaders
        private void LoadMembers()
        {
            cmbStudent.Items.Clear();
            cmbStudent.Items.Add("All");
            cmbStudent.SelectedIndex = 0;

            using (var con = Db.GetConnection())
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT MemberId, FirstName || ' ' || LastName AS FullName FROM Members", con))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbStudent.Items.Add(new ComboBoxItem
                        {
                            Id = Convert.ToInt32(reader["MemberId"]),
                            Name = reader["FullName"].ToString()
                        });
                    }
                }
            }
        }
        private void LoadBooks()
        {
            cmbBook.Items.Clear();
            cmbBook.Items.Add("All");
            cmbBook.SelectedIndex = 0;

            using (var con = Db.GetConnection())
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("SELECT BookId, Title FROM Books", con))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbBook.Items.Add(new ComboBoxItem
                        {
                            Id = Convert.ToInt32(reader["BookId"]),
                            Name = reader["Title"].ToString()
                        });
                    }
                }
            }
        }


        // Report loaders
        private void LoadDashboardReport()
        {
            dgvReports.Rows.Clear();

            using (var con = Db.GetConnection())
            {
                con.Open();

                int totalBooks = 0, totalMembers = 0, borrowedBooks = 0, overdueBooks = 0;

                // Total Books
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Books", con))
                    totalBooks = Convert.ToInt32(cmd.ExecuteScalar());

                // Total Members
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Members", con))
                    totalMembers = Convert.ToInt32(cmd.ExecuteScalar());

                // Borrowed Books
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Borrowings WHERE ReturnDate IS NULL", con))
                    borrowedBooks = Convert.ToInt32(cmd.ExecuteScalar());

                // Overdue Books
                using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Borrowings WHERE ReturnDate IS NULL AND DueDate < @Today", con))
                {
                    cmd.Parameters.AddWithValue("@Today", DateTime.Now.Date);
                    overdueBooks = Convert.ToInt32(cmd.ExecuteScalar());
                }

                dgvReports.Rows.Add(totalBooks, totalMembers, borrowedBooks, overdueBooks);
            }
        }

        // TODO: Implement all other report loaders here similar to your existing methods:
        // LoadBorrowingReport, LoadReturnsReport, LoadStudentActivityReport, LoadBooksReport, LoadMembersReport, LoadCirculationReport, LoadOverdueReport, LoadPenaltyReport

        // Report loaders
        private void LoadBorrowingReport()
        {
            dgvReports.Rows.Clear();
            dgvReports.Columns.Clear();

            // Add columns
            dgvReports.Columns.Add("MemberName", "Students Name / ID");
            dgvReports.Columns.Add("BookTitle", "Book Title / ID");
            dgvReports.Columns.Add("BorrowDate", "Borrow Date");
            dgvReports.Columns.Add("DueDate", "Due Date");
            dgvReports.Columns.Add("Status", "Status");

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();

                string sql = @"
            SELECT m.MemberId || ' - ' || m.FirstName || ' ' || m.LastName AS MemberName,
                   b.BookId || ' - ' || b.Title AS BookTitle,
                   br.BorrowDate,
                   br.DueDate,
                   br.Status
            FROM Borrowings br
            JOIN Members m ON br.MemberId = m.MemberId
            JOIN Books b ON br.BookId = b.BookId
            WHERE (@Student = 0 OR m.MemberId = @Student)
              AND (@Book = 0 OR b.BookId = @Book)
              AND br.BorrowDate BETWEEN @From AND @To;
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    // Use 0 if "All" is selected
                    int studentId = (cmbStudent.SelectedItem is ComboBoxItem studentItem) ? studentItem.Id : 0;
                    int bookId = (cmbBook.SelectedItem is ComboBoxItem bookItem) ? bookItem.Id : 0;

                    cmd.Parameters.AddWithValue("@Student", studentId);
                    cmd.Parameters.AddWithValue("@Book", bookId);
                    cmd.Parameters.AddWithValue("@From", dtpFrom.Value.Date);
                    cmd.Parameters.AddWithValue("@To", dtpTo.Value.Date);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                reader["MemberName"],
                                reader["BookTitle"],
                                Convert.ToDateTime(reader["BorrowDate"]).ToString("yyyy-MM-dd"),
                                Convert.ToDateTime(reader["DueDate"]).ToString("yyyy-MM-dd"),
                                reader["Status"]
                            );
                        }
                    }
                }
            }
        }

        private void LoadReturnsReport()
        {
            dgvReports.Rows.Clear();
            dgvReports.Columns.Clear();

            // Add columns
            dgvReports.Columns.Add("MemberName", "Students");
            dgvReports.Columns.Add("BookTitle", "Book");
            dgvReports.Columns.Add("BorrowDate", "Borrowed On");
            dgvReports.Columns.Add("ReturnDate", "Returned On");
            dgvReports.Columns.Add("Status", "Status");

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();
                string sql = @"
            SELECT m.FirstName || ' ' || m.LastName AS MemberName,
                   b.Title AS BookTitle,
                   br.BorrowDate,
                   br.ReturnDate,
                   br.Status
            FROM Borrowings br
            JOIN Members m ON br.MemberId = m.MemberId
            JOIN Books b ON br.BookId = b.BookId
            WHERE br.ReturnDate IS NOT NULL
              AND (@Student = 'All' OR m.MemberId = @Student)
              AND (@Book = 'All' OR b.BookId = @Book)
              AND br.BorrowDate BETWEEN @From AND @To;
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Student", cmbStudent.SelectedItem is ComboBoxItem item1 ? item1.Id : (object)"All");
                    cmd.Parameters.AddWithValue("@Book", cmbBook.SelectedItem is ComboBoxItem item2 ? item2.Id : (object)"All");
                    cmd.Parameters.AddWithValue("@From", dtpFrom.Value.Date);
                    cmd.Parameters.AddWithValue("@To", dtpTo.Value.Date);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                reader["MemberName"],
                                reader["BookTitle"],
                                reader["BorrowDate"],
                                reader["ReturnDate"],
                                reader["Status"]
                            );
                        }
                    }
                }
            }
        }
        private void LoadStudentActivityReport()
        {
            dgvReports.Rows.Clear();

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();
                string sql = @"
            SELECT m.FirstName || ' ' || m.LastName AS MemberName,
                   (SELECT COUNT(*) FROM Borrowings br WHERE br.MemberId = m.MemberId) AS BorrowedBooks,
                   (SELECT COUNT(*) FROM Borrowings br WHERE br.MemberId = m.MemberId AND br.ReturnDate IS NOT NULL) AS ReturnedBooks,
                   (SELECT COUNT(*) FROM Borrowings br WHERE br.MemberId = m.MemberId AND br.ReturnDate IS NULL) AS PendingBorrowings,
                   CASE WHEN m.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
            FROM Members m
            WHERE (@Student = 'All' OR m.MemberId = @Student);
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Student", cmbStudent.SelectedItem is ComboBoxItem item1 ? item1.Id : (object)"All");

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                reader["MemberName"],
                                reader["BorrowedBooks"],
                                reader["ReturnedBooks"],
                                reader["PendingBorrowings"],
                                reader["Status"]
                            );
                        }
                    }
                }
            }
        }

        private void LoadBooksReport()
        {
            dgvReports.Rows.Clear();

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();
                string sql = @"
            SELECT BookId, Title, Author, Status
            FROM Books
            WHERE Title LIKE @TitleFilter;
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@TitleFilter", "%");
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                 reader["Title"],   // show actual Title
                                 reader["Author"],
                                 reader["Status"]
                             );
                        }
                    }
                }
            }
        }

        private void LoadMembersReport()
        {
            dgvReports.Rows.Clear();   // keep columns, only clear rows

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();

                string sql = @"
            SELECT FirstName || ' ' || LastName AS FullName,
                   CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status
            FROM Members
            WHERE (FirstName || ' ' || LastName) LIKE @NameFilter;
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@NameFilter", "%");

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                reader["FullName"],
                                reader["Status"]
                            );
                        }
                    }
                }
            }
        }



        private void LoadOverdueReport()
        {
            dgvReports.Rows.Clear();

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();
                string sql = @"
            SELECT m.FirstName || ' ' || m.LastName AS MemberName,
                   b.Title AS BookTitle,
                   br.DueDate,
                   julianday('now') - julianday(br.DueDate) AS DaysOverdue,
                   br.Status
            FROM Borrowings br
            JOIN Members m ON br.MemberId = m.MemberId
            JOIN Books b ON br.BookId = b.BookId
            WHERE br.ReturnDate IS NULL
              AND br.DueDate < @Today
              AND (@Student = 'All' OR m.MemberId = @Student)
              AND (@Book = 'All' OR b.BookId = @Book);
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Today", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@Student", cmbStudent.SelectedItem is ComboBoxItem item1 ? item1.Id : (object)"All");
                    cmd.Parameters.AddWithValue("@Book", cmbBook.SelectedItem is ComboBoxItem item2 ? item2.Id : (object)"All");

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                reader["MemberName"],
                                reader["BookTitle"],
                                reader["DueDate"],
                                reader["DaysOverdue"],
                                reader["Status"]
                            );
                        }
                    }
                }
            }
        }

        private void LoadPenaltyReport()
        {
            dgvReports.Rows.Clear();
            dgvReports.Columns.Clear();

            // Add columns
            dgvReports.Columns.Add("MemberName", "Students");
            dgvReports.Columns.Add("BookTitle", "Book");
            dgvReports.Columns.Add("Amount", "Amount");
            dgvReports.Columns.Add("Status", "Status");
            dgvReports.Columns.Add("CreatedAt", "Date Issued");

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();
                string sql = @"
            SELECT m.FirstName || ' ' || m.LastName AS MemberName,
                   b.Title AS BookTitle,
                   p.Amount,
                   p.Status,
                   p.CreatedAt
            FROM Penalties p
            JOIN Borrowings br ON p.BorrowId = br.BorrowId
            JOIN Members m ON p.MemberId = m.MemberId
            JOIN Books b ON br.BookId = b.BookId
            WHERE (@Student = 'All' OR m.MemberId = @Student)
              AND (@Book = 'All' OR b.BookId = @Book)
              AND (strftime('%Y-%m', p.CreatedAt) = strftime('%Y-%m', 'now') OR p.Status = 'Unpaid')
            ORDER BY p.CreatedAt DESC;
        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Student", cmbStudent.SelectedItem is ComboBoxItem item1 ? item1.Id : (object)"All");
                    cmd.Parameters.AddWithValue("@Book", cmbBook.SelectedItem is ComboBoxItem item2 ? item2.Id : (object)"All");

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dgvReports.Rows.Add(
                                reader["MemberName"],
                                reader["BookTitle"],
                                reader["Amount"],
                                reader["Status"],
                                Convert.ToDateTime(reader["CreatedAt"]).ToString("yyyy-MM-dd")
                            );
                        }
                    }
                }
            }
        }


        private void BtnExport_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export feature not implemented yet.");
        }

        private void dgvReports_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }
    }

    // Helper class for ComboBox items
    public class ComboBoxItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}
