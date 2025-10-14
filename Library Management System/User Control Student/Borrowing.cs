using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control_Student
{
    public partial class Borrowing : UserControl
    {
        private string _studentNo;

        public string StudentNo
        {
            get { return _studentNo; }
            set
            {
                _studentNo = value;
                LoadBorrowedBooks();
            }
        }

        // Event to notify other controls
        public static event Action BookReturned;

        public Borrowing()
        {
            InitializeComponent();
        }

        private void LoadBorrowedBooks()
        {
            if (string.IsNullOrEmpty(_studentNo)) return;

            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                    SELECT 
                        br.BorrowId,
                        b.Title, 
                        br.BorrowDate, 
                        br.DueDate,
                        CASE 
                            WHEN br.ReturnDate IS NULL THEN 'Not Returned' 
                            ELSE 'Returned' 
                        END AS Status
                    FROM Borrowings br
                    INNER JOIN Books b ON b.BookId = br.BookId
                    INNER JOIN Members m ON m.MemberId = br.MemberId
                    WHERE m.StudentNo = @studentNo
                    ORDER BY br.BorrowDate DESC";

                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", _studentNo);

                    using (var adapter = new System.Data.SQLite.SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dgvBorrowedBooks.DataSource = dt;

                        // Hide BorrowId column
                        if (dgvBorrowedBooks.Columns.Contains("BorrowId"))
                            dgvBorrowedBooks.Columns["BorrowId"].Visible = false;

                        ColorStatusText();
                    }
                }
            }
        }

        private void ColorStatusText()
        {
            foreach (DataGridViewRow row in dgvBorrowedBooks.Rows)
            {
                if (row.Cells["Status"].Value != null)
                {
                    string status = row.Cells["Status"].Value.ToString();
                    if (status == "Not Returned")
                        row.Cells["Status"].Style.ForeColor = Color.Red;
                    else if (status == "Returned")
                        row.Cells["Status"].Style.ForeColor = Color.Green;
                }
            }
        }

        private void BtnReturnBook_Click(object sender, EventArgs e)
        {
            if (dgvBorrowedBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("⚠️ Please select a book to return.");
                return;
            }

            var selectedRow = dgvBorrowedBooks.SelectedRows[0];
            string status = selectedRow.Cells["Status"].Value.ToString();

            if (status == "Returned")
            {
                MessageBox.Show("ℹ️ This book has already been returned.");
                return;
            }

            int borrowId = Convert.ToInt32(selectedRow.Cells["BorrowId"].Value);

            using (var con = Db.GetConnection())
            {
                con.Open();
                string updateQuery = "UPDATE Borrowings SET ReturnDate = @returnDate, Status = 'Pending' WHERE BorrowId = @borrowId";


                using (var cmd = new System.Data.SQLite.SQLiteCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@returnDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@borrowId", borrowId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("✅ Book returned successfully!");
            LoadBorrowedBooks();

            // Trigger event so other controls can refresh
            BookReturned?.Invoke();
        }

        private void Borrowing_Load(object sender, EventArgs e)
        {
            // When the control loads, if StudentNo is already set, load data
            if (!string.IsNullOrEmpty(_studentNo))
            {
                LoadBorrowedBooks();
            }
        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
