using System;
using System.Data.SQLite;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Forms
{
    public partial class StudentForm : Form
    {
        private string _studentNo;

        public StudentForm(string studentNo)
        {
            InitializeComponent();
            _studentNo = studentNo;
            LoadStudentInfo();
        }

        private void LoadStudentInfo()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT StudentNo, FirstName, LastName, Course, YearLevel FROM Members WHERE StudentNo=@studentNo";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", _studentNo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblStudentNo.Text = reader["StudentNo"].ToString();
                            lblName.Text = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                            lblCourse.Text = reader["Course"].ToString();
                            lblYearLevel.Text = reader["YearLevel"].ToString();
                        }
                    }
                }
            }
        }

        private void lblStudentNo_Click(object sender, EventArgs e)
        {

        }

        private void lblName_Click(object sender, EventArgs e)
        {

        }

        private void lblCourse_Click(object sender, EventArgs e)
        {

        }

        private void lblYearLevel_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void StudentForm_Load(object sender, EventArgs e)
        {

        }
    }
}
