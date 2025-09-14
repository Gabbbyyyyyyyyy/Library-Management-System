using System;
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
        public ManageMembersControl()
        {
            InitializeComponent();

            dgvMembers.CellFormatting += dgvMembers_CellFormatting;
            dgvMembers.SelectionChanged += dgvMembers_SelectionChanged;

            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            DataGridViewHelper.ApplyDefaultStyle(dgvMembers);

            // Apply global searchbox helper
            TextBoxHelper.ApplySearchBox(txtSearch, "Search members...", txtSearch_KeyDown);
            txtSearch.TextChanged += txtSearch_TextChanged;

            // Optional: hide no-results label initially
            lblSearchMessage.Text = "";
        }

        private void ManageMembersControl_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                LoadMembers();
            }));
        }

        // Search members live
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadMembers(txtSearch.Text.Trim());
        }

        // Called when Enter is pressed in the search TextBox
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
                        MemberId, 
                        StudentNo, 
                        FirstName, 
                        LastName, 
                        Course, 
                        YearLevel,
                        CASE 
                            WHEN IsActive = 1 THEN 'Active' 
                            ELSE 'Inactive' 
                        END AS Status
                    FROM Members";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " WHERE StudentNo LIKE @search OR FirstName LIKE @search OR LastName LIKE @search OR Course LIKE @search";
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

                        // Set consistent row height
                        foreach (DataGridViewRow row in dgvMembers.Rows)
                        {
                            row.Height = 40; // Adjust height as needed
                        }

                        // Clear selection
                        dgvMembers.ClearSelection();
                        dgvMembers.CurrentCell = null;
                        btnDeactivate.Enabled = false;
                        btnReactivate.Enabled = false;

                        // Show message if no results
                        lblSearchMessage.Text = dt.Rows.Count == 0 ? "No members match your search." : "";
                    }
                }
            }
        }

        private void dgvMembers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMembers.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.CellStyle.ForeColor = e.Value.ToString() == "Inactive" ? Color.Red : Color.Green;
            }
        }

        private void dgvMembers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count > 0)
            {
                string status = dgvMembers.SelectedRows[0].Cells["Status"].Value.ToString();
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
                string details = $"Student No: {row.Cells["StudentNo"].Value}\n" +
                                 $"Name: {row.Cells["FirstName"].Value} {row.Cells["LastName"].Value}\n" +
                                 $"Course: {row.Cells["Course"].Value}\n" +
                                 $"Year Level: {row.Cells["YearLevel"].Value}\n" +
                                 $"Status: {row.Cells["Status"].Value}";
                MessageBox.Show(details, "Member Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a member to deactivate.");
                return;
            }

            int memberId = Convert.ToInt32(dgvMembers.SelectedRows[0].Cells["MemberId"].Value);
            using (var con = Db.GetConnection())
            {
                con.Open();
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
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a member to reactivate.");
                return;
            }

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
        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a member to edit.");
                return;
            }

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
