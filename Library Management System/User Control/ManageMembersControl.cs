using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem
{
    public partial class ManageMembersControl : UserControl
    {
        private Timer refreshTimer;

        public ManageMembersControl()
        {
            InitializeComponent();

            dgvMembers.CellFormatting += dgvMembers_CellFormatting;
            dgvMembers.SelectionChanged += dgvMembers_SelectionChanged;
            dgvMembers.CellDoubleClick += dgvMembers_CellDoubleClick;

            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.MultiSelect = false;
            dgvMembers.ClearSelection();
            dgvMembers.CurrentCell = null;
            dgvMembers.SelectionChanged += (s, e) => dgvMembers.ClearSelection();


            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            DataGridViewHelper.ApplyDefaultStyle(dgvMembers);
            TextBoxHelper.ApplySearchBox(txtSearch, "Search members...", txtSearch_KeyDown);
            txtSearch.TextChanged += txtSearch_TextChanged;
            lblSearchMessage.Text = "";

            // Timer to refresh every 3 seconds for real-time HasPendingBorrow
            refreshTimer = new Timer();
            refreshTimer.Interval = 3000; // 3 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();


        }

        private void ManageMembersControl_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                LoadMembers();
            }));
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                using (var con = Db.GetConnection())
                {
                    con.Open();

                    // Get actual borrow status of all members
                    string query = @"
                SELECT 
                    m.MemberId,
                    CASE 
                        WHEN EXISTS (
                            SELECT 1 
                            FROM Borrowings b 
                            WHERE b.MemberId = m.MemberId 
                            AND (b.ReturnDate IS NULL OR b.ReturnDate = '')
                        ) THEN 'Borrowing'
                        ELSE 'No Borrowings'
                    END AS HasPendingBorrow
                FROM Members m";

                    using (var cmd = new SQLiteCommand(query, con))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var statusDict = new Dictionary<int, string>();

                        while (reader.Read())
                        {
                            int memberId = Convert.ToInt32(reader["MemberId"]);
                            string status = reader["HasPendingBorrow"].ToString();
                            statusDict[memberId] = status;
                        }

                        foreach (DataGridViewRow row in dgvMembers.Rows)
                        {
                            if (row.Cells["MemberId"].Value == null) continue;

                            int memberId = Convert.ToInt32(row.Cells["MemberId"].Value);
                            if (statusDict.TryGetValue(memberId, out string newStatus))
                            {
                                row.Cells["HasPendingBorrow"].Value = newStatus;
                                row.Cells["HasPendingBorrow"].Style.ForeColor =
                                    newStatus == "Borrowing" ? Color.OrangeRed : Color.Green;
                            }
                        }
                    }
                }
                // Prevent row selection after refresh
                dgvMembers.ClearSelection();
                dgvMembers.CurrentCell = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating HasPendingBorrow: " + ex.Message);
            }
        }




        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadMembers(txtSearch.Text.Trim());
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadMembers(txtSearch.Text.Trim());
            }
        }

        private void LoadMembers(string searchText = "")
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
            SELECT 
                m.MemberId, 
                m.StudentNo, 
                m.FirstName, 
                m.LastName, 
                m.Course, 
                m.YearLevel,
                CASE WHEN m.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
                CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM Borrowings b 
                        WHERE b.MemberId = m.MemberId AND (b.ReturnDate IS NULL OR b.ReturnDate = '')
                    ) THEN 'Borrowing'
                    ELSE 'No Borrowings'
                END AS HasPendingBorrow
            FROM Members m";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " WHERE (StudentNo LIKE @search OR FirstName LIKE @search OR LastName LIKE @search OR Course LIKE @search)";
                }

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrWhiteSpace(searchText))
                        cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvMembers.DataSource = dt;

                        // Format rows and add color for HasPendingBorrow
                        foreach (DataGridViewRow row in dgvMembers.Rows)
                        {
                            row.Height = 40;

                            string pending = row.Cells["HasPendingBorrow"].Value.ToString();
                            row.Cells["HasPendingBorrow"].Style.ForeColor =
                                pending == "Borrowing" ? Color.Red : Color.Black;
                        }

                        // ✅ Ensure no row stays selected
                        dgvMembers.ClearSelection();
                        dgvMembers.CurrentCell = null;
                        dgvMembers.SelectionChanged += (s, e) => dgvMembers.ClearSelection();


                        btnDeactivate.Enabled = false;
                        btnReactivate.Enabled = false;

                        lblSearchMessage.Text = dt.Rows.Count == 0 ? "No members match your search." : "";

                    }
                }
            }
        }


        private void dgvMembers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMembers.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                e.CellStyle.ForeColor = e.Value.ToString() == "Inactive" ? Color.Red : Color.Green;

            if (dgvMembers.Columns[e.ColumnIndex].Name == "HasPendingBorrow" && e.Value != null)
                e.CellStyle.ForeColor = e.Value.ToString() == "Borrowing" ? Color.Red : Color.Black;

        }

        private void dgvMembers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count > 0)
            {
                string status = dgvMembers.SelectedRows[0].Cells["Status"].Value?.ToString() ?? "";

                btnDeactivate.Enabled = status == "Active";
                btnReactivate.Enabled = status == "Inactive";
            }
            else
            {
                btnDeactivate.Enabled = false;
                btnReactivate.Enabled = false;
            }
        }

        private void dgvMembers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvMembers.Rows[e.RowIndex];
                int memberId = Convert.ToInt32(row.Cells["MemberId"].Value);

                // Always recheck latest borrow status
                string borrowStatus = "No Borrowings";
                string borrowedBooks = "";

                using (var con = Db.GetConnection())
                {
                    con.Open();

                    // Check if the student has active borrowings
                    string statusQuery = @"
                SELECT COUNT(*) 
                FROM Borrowings 
                WHERE MemberId = @id 
                AND (ReturnDate IS NULL OR ReturnDate = '')";
                    using (var cmd = new SQLiteCommand(statusQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", memberId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        borrowStatus = count > 0 ? "Borrowing" : "No Borrowings";
                    }

                    // If borrowing, fetch book titles
                    if (borrowStatus == "Borrowing")
                    {
                        string titleQuery = @"
                    SELECT b.Title
                    FROM Borrowings br
                    JOIN Books b ON br.BookId = b.BookId
                    WHERE br.MemberId = @id 
                    AND (br.ReturnDate IS NULL OR br.ReturnDate = '')";
                        using (var cmd = new SQLiteCommand(titleQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@id", memberId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    borrowedBooks += "• " + reader["Title"].ToString() + "\n";
                            }
                        }
                    }
                }

                // Update grid cell immediately to stay consistent
                row.Cells["HasPendingBorrow"].Value = borrowStatus;
                row.Cells["HasPendingBorrow"].Style.ForeColor =
    borrowStatus == "Borrowing" ? Color.Red : Color.Black;


                // Show message
                string details = $"Student No: {row.Cells["StudentNo"].Value}\n" +
                                 $"Name: {row.Cells["FirstName"].Value} {row.Cells["LastName"].Value}\n" +
                                 $"Course: {row.Cells["Course"].Value}\n" +
                                 $"Year Level: {row.Cells["YearLevel"].Value}\n" +
                                 $"Status: {row.Cells["Status"].Value}\n" +
                                 $"Borrow Status: {borrowStatus}\n\n";

                if (borrowStatus == "Borrowing" && !string.IsNullOrEmpty(borrowedBooks))
                    details += "📚 Borrowed Books:\n" + borrowedBooks;
                else
                    details += "📘 No borrowed books.";

                MessageBox.Show(details, "Member Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }




        private void dgvMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int memberId = Convert.ToInt32(dgvMembers.Rows[e.RowIndex].Cells["MemberId"].Value);

                using (var con = Db.GetConnection())
                {
                    con.Open();
                    string query = @"SELECT b.Title
                                     FROM Borrowings br
                                     JOIN Books b ON br.BookId = b.BookId
                                     WHERE br.MemberId = @id AND (br.ReturnDate IS NULL OR br.ReturnDate = '')";

                    using (var cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", memberId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            string borrowedBooks = "";
                            while (reader.Read())
                                borrowedBooks += "- " + reader["Title"].ToString() + "\n";

                            if (string.IsNullOrEmpty(borrowedBooks))
                                borrowedBooks = "No pending borrowed books.";

                            MessageBox.Show(borrowedBooks, "Borrowed Books");
                        }
                    }
                }
            }
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0) return;

            int memberId = Convert.ToInt32(dgvMembers.SelectedRows[0].Cells["MemberId"].Value);

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Check if member has any active borrowings (ReturnDate IS NULL)
                string checkQuery = "SELECT COUNT(*) FROM Borrowings WHERE MemberId = @memberId AND ReturnDate IS NULL";
                using (var cmd = new SQLiteCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    int activeBorrows = Convert.ToInt32(cmd.ExecuteScalar());

                    if (activeBorrows > 0)
                    {
                        MessageBox.Show(
                            "This member cannot be deactivated because they still have borrowed books.",
                            "Action Denied",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }
                }

                // If no active borrowings, deactivate the member
                using (var cmd = new SQLiteCommand("UPDATE Members SET IsActive = 0 WHERE MemberId = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member deactivated successfully!");
            LoadMembers(txtSearch.Text.Trim());
        }


        private void btnReactivate_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0) return;

            int memberId = Convert.ToInt32(dgvMembers.SelectedRows[0].Cells["MemberId"].Value);
            using (var con = Db.GetConnection())
            {
                con.Open();
                using (var cmd = new SQLiteCommand("UPDATE Members SET IsActive = 1 WHERE MemberId = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member reactivated successfully!");
            LoadMembers(txtSearch.Text.Trim());
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Step 1: Ensure a book row is selected
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a member to edit.");
                return;
            }
            if (dgvMembers.SelectedRows.Count == 0) return;

            var row = dgvMembers.SelectedRows[0];
            int memberId = Convert.ToInt32(row.Cells["MemberId"].Value);
            string firstName = row.Cells["FirstName"].Value.ToString();
            string lastName = row.Cells["LastName"].Value.ToString();
            string course = row.Cells["Course"].Value.ToString();
            string yearLevel = row.Cells["YearLevel"].Value.ToString();

            MemberEditForm editForm = new MemberEditForm(memberId, firstName, lastName, course, yearLevel);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadMembers(txtSearch.Text.Trim());
            }
        }
    }
}
