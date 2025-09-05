using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem
{
    public partial class MembersForm : Form
    {
        public MembersForm()
        {
            InitializeComponent();
            LoadMembers();
        }

        private void LoadMembers(string search = "")
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT MemberId, StudentNo, FullName, Course, YearLevel FROM Members";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE StudentNo LIKE @search OR FullName LIKE @search OR Course LIKE @search";
                }

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvMembers.DataSource = dt;
                    }
                }
            }

            ClearInputs();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string fullName = $"{txtFirstName.Text} {txtLastName.Text}".Trim();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "INSERT INTO Members (StudentNo, FirstName, LastName, Course, YearLevel) " +
                "VALUES (@studentNo, @firstName, @lastName, @course, @yearLevel)";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", txtStudentNo.Text);
                    cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@lastName", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@course", txtCourse.Text);
                    cmd.Parameters.AddWithValue("@yearLevel", txtYearLevel.Text);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member added successfully!");
            LoadMembers();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a member to update.");
                return;
            }

            int memberId = Convert.ToInt32(dgvMembers.SelectedRows[0].Cells["MemberId"].Value);
            string fullName = $"{txtFirstName.Text} {txtLastName.Text}".Trim();
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "UPDATE Members SET StudentNo=@studentNo, FullName=@fullName, Course=@course, YearLevel=@yearLevel " +
                               "WHERE MemberId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", txtStudentNo.Text);
                    cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@lastName", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@course", txtCourse.Text);
                    cmd.Parameters.AddWithValue("@yearLevel", txtYearLevel.Text);
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member updated successfully!");
            LoadMembers();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a member to delete.");
                return;
            }

            int memberId = Convert.ToInt32(dgvMembers.SelectedRows[0].Cells["MemberId"].Value);

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "DELETE FROM Members WHERE MemberId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member deleted successfully!");
            LoadMembers();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadMembers(txtStudentNo.Text); // search by first name (you can change to txtStudentNo if preferred)
        }

        private void dgvMembers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtStudentNo.Text = dgvMembers.Rows[e.RowIndex].Cells["StudentNo"].Value.ToString();
                txtFirstName.Text = dgvMembers.Rows[e.RowIndex].Cells["FirstName"].Value.ToString();
                txtLastName.Text = dgvMembers.Rows[e.RowIndex].Cells["LastName"].Value.ToString();
                txtCourse.Text = dgvMembers.Rows[e.RowIndex].Cells["Course"].Value.ToString();
                txtYearLevel.Text = dgvMembers.Rows[e.RowIndex].Cells["YearLevel"].Value.ToString();

                string fullName = dgvMembers.Rows[e.RowIndex].Cells["FullName"].Value.ToString();
                string[] parts = fullName.Split(' ');

                if (parts.Length > 1)
                {
                    txtFirstName.Text = parts[0];
                    txtLastName.Text = string.Join(" ", parts.Skip(1));
                }
                else
                {
                    txtFirstName.Text = fullName;
                    txtLastName.Text = "";
                }
            }
        }

        private void ClearInputs()
        {
            txtStudentNo.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            txtCourse.Clear();
            txtYearLevel.Clear();
        }

        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtLastName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
