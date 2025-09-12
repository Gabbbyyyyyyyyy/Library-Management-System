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

        private void LoadMembers(string txtSearch = "")
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT MemberId, StudentNo, FirstName,LastName, Course, YearLevel FROM Members";

                if (!string.IsNullOrWhiteSpace(txtSearch))
                {
                    query += " WHERE StudentNo LIKE @search OR FirstName LIKE @search OR LastName LIKE @search OR Course LIKE @search";
                }

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrWhiteSpace(txtSearch))
                        cmd.Parameters.AddWithValue("@search", "%" + txtSearch + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvMembers.DataSource = dt;
                    }
                }
            }
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

        
        private void dgvMembers_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }



        private void MembersForm_Load(object sender, EventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

        }
    }
}
