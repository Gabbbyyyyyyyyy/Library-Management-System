using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control_Student
{


    public partial class AvailbleCopies : UserControl
    {
        private string _studentNo;
        private DataTable _booksTable; // keep data for filtering

        public string StudentNo
        {
            get => _studentNo;
            set
            {
                _studentNo = value;
                LoadAvailableCopies();
            }
        }

        public AvailbleCopies()
        {
            InitializeComponent();

            // Let parent panel control size
            this.Dock = DockStyle.Fill;

            // Configure DataGridView
            dgvAvailableCopies.Dock = DockStyle.Fill;
            dgvAvailableCopies.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvAvailableCopies.ReadOnly = true;
            dgvAvailableCopies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAvailableCopies.MultiSelect = false;
            dgvAvailableCopies.AllowUserToAddRows = false;
            dgvAvailableCopies.AllowUserToDeleteRows = false;
            dgvAvailableCopies.AllowUserToResizeRows = false;
            dgvAvailableCopies.AllowUserToResizeColumns = false;
            dgvAvailableCopies.AllowUserToOrderColumns = false;
            dgvAvailableCopies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // ✅ Header style
            dgvAvailableCopies.EnableHeadersVisualStyles = false;
            dgvAvailableCopies.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
            dgvAvailableCopies.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAvailableCopies.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Regular);

            dgvAvailableCopies.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvAvailableCopies.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dgvAvailableCopies.DefaultCellStyle.SelectionForeColor = Color.Black;

            // ✅ Alternate row colors
            dgvAvailableCopies.RowsDefaultCellStyle.BackColor = Color.White;
            dgvAvailableCopies.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            // Configure DataGridView design
            dgvAvailableCopies.ReadOnly = true;
            dgvAvailableCopies.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAvailableCopies.AllowUserToAddRows = false;
            dgvAvailableCopies.AllowUserToDeleteRows = false;
            dgvAvailableCopies.AllowUserToResizeRows = false;
            dgvAvailableCopies.RowTemplate.Height = 40; // ✅ Row height

            // Prevent auto-selection after loading data
            dgvAvailableCopies.DataBindingComplete += (s, e) => dgvAvailableCopies.ClearSelection();

            SendMessage(textBox1.Handle, EM_SETCUEBANNER, 0, "Search...");
            textBox1.KeyDown += txtSearch_KeyDown;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private void AvailbleCopies_Load(object sender, EventArgs e)
        {
            // Optional: LoadAvailableCopies();
        }

        private void LoadAvailableCopies()
        {
            try
            {
                using (var con = Db.GetConnection())
                {
                    con.Open();

                    string query = @"
                SELECT 
                    b.BookId,
                    b.ISBN,
                    b.Title,
                    b.Author,
                    b.Category,
                    b.Quantity,
                    b.AvailableCopies,
                    CASE
                        WHEN b.AvailableCopies > 0 THEN 'Available'
                        WHEN EXISTS (
                            SELECT 1 FROM Reservations r 
                            WHERE r.BookId = b.BookId AND r.Status = 'Active'
                        ) THEN 'Reserved'
                        ELSE 'Not Available'
                    END AS Status
                FROM Books b;";

                    using (var cmd = new SQLiteCommand(query, con))
                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        _booksTable = new DataTable();
                        da.Fill(_booksTable);

                        dgvAvailableCopies.DataSource = _booksTable.DefaultView;

                        // rename headers for clarity
                        dgvAvailableCopies.Columns["BookId"].HeaderText = "ID";
                        dgvAvailableCopies.Columns["ISBN"].HeaderText = "ISBN";
                        dgvAvailableCopies.Columns["Title"].HeaderText = "Title";
                        dgvAvailableCopies.Columns["Author"].HeaderText = "Author";
                        dgvAvailableCopies.Columns["Category"].HeaderText = "Category";
                        dgvAvailableCopies.Columns["Quantity"].HeaderText = "Total Quantity";
                        dgvAvailableCopies.Columns["AvailableCopies"].HeaderText = "Available Copies";
                        dgvAvailableCopies.Columns["Status"].HeaderText = "Status";

                        ColorStatusColumnText();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading available copies: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void dgvAvailableCopies_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Example: if you added a "Return" button column
            if (e.RowIndex >= 0 && dgvAvailableCopies.Columns[e.ColumnIndex].Name == "ReturnColumn")
            {
                // Handle book return logic here
                int bookId = Convert.ToInt32(dgvAvailableCopies.Rows[e.RowIndex].Cells["BookId"].Value);

                MessageBox.Show($"Returning book with ID {bookId}");
                // TODO: update DB and refresh grid
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"SELECT BookId, ISBN, Title, Author, Category, Quantity, AvailableCopies,
                                CASE WHEN AvailableCopies = 0 THEN 'Borrowed Out' ELSE 'Available' END AS Status
                         FROM Books
                         WHERE Title LIKE @search 
                            OR Author LIKE @search 
                            OR Category LIKE @search 
                            OR ISBN LIKE @search";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvAvailableCopies.DataSource = dt;

                        // Use the DataTable to check if any rows exist
                        if (dt.Rows.Count == 0)
                        {
                            lblSearchMessage.Text = "No books match your search.";
                        }
                        else
                        {
                            lblSearchMessage.Text = "";
                        }
                    }
                }
            }
        }
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Only trigger search on Enter key
            {
                e.SuppressKeyPress = true; // Prevent ding sound
                PerformSearch();
            }
        }
        private void PerformSearch()
        {
            string searchText = textBox1.Text.Trim();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"SELECT BookId, ISBN, Title, Author, Category, Quantity, AvailableCopies,
                        CASE WHEN AvailableCopies = 0 THEN 'Borrowed Out' ELSE 'Available' END AS Status
                        FROM Books
                        WHERE Title LIKE @search 
                           OR Author LIKE @search 
                           OR Category LIKE @search 
                           OR ISBN LIKE @search";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvAvailableCopies.DataSource = dt;

                        dgvAvailableCopies.DataSource = dt; // Always update DataGridView

                        if (dt.Rows.Count == 0)
                        {
                            //MessageBox.Show("No books found matching your search.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }




        private void ColorStatusColumnText()
        {
            foreach (DataGridViewRow row in dgvAvailableCopies.Rows)
            {
                if (row.Cells["Status"].Value != null)
                {
                    string status = row.Cells["Status"].Value.ToString();
                    switch (status)
                    {
                        case "Available":
                            row.Cells["Status"].Style.ForeColor = Color.Green;
                            row.Cells["Status"].Style.Font = new Font(dgvAvailableCopies.Font, FontStyle.Bold);
                            break;
                        case "Reserved":
                            row.Cells["Status"].Style.ForeColor = Color.Orange;
                            row.Cells["Status"].Style.Font = new Font(dgvAvailableCopies.Font, FontStyle.Bold);
                            break;
                        case "Not Available":
                        case "Borrowed Out":
                            row.Cells["Status"].Style.ForeColor = Color.Red;
                            row.Cells["Status"].Style.Font = new Font(dgvAvailableCopies.Font, FontStyle.Bold);
                            break;
                    }
                }
            }
        }


        private void labelTitle_Click(object sender, EventArgs e)
        {

        }

      
    }
}