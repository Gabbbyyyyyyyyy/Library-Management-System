using System;
using System.Windows.Forms;
using Library_Management_System.User_Control_Student;
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

            // ✅ Load AvailbleCopies on startup
            AvailbleCopies availableCopiesUC = new AvailbleCopies
            {
                StudentNo = _studentNo, // if you’re passing student number
                Dock = DockStyle.Fill
            };

            panelContent.Controls.Clear();
            panelContent.Controls.Add(availableCopiesUC);
        }


        private void LoadControl(UserControl control)
        {
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Clear();
            panelContent.Controls.Add(control);
        }

        private void StudentForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            // Load AvailableCopies by default
            LoadControl(new AvailbleCopies { StudentNo = _studentNo });
        }

        private void btnAvailableCopies_Click(object sender, EventArgs e)
        {
            LoadControl(new AvailbleCopies { StudentNo = _studentNo });
        }

        private void btnBorrowing_Click(object sender, EventArgs e)
        {
            LoadControl(new User_Control_Student.Borrowing { StudentNo = _studentNo });
        }

       

       

        private void LoadStudentInfo()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT StudentNo, FirstName, LastName, Course, YearLevel, IsActive FROM Members WHERE StudentNo=@studentNo";

                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", _studentNo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isActive = Convert.ToInt32(reader["IsActive"]) == 1;
                            if (!isActive)
                            {
                                MessageBox.Show(
                                    "⚠️ Your account has been deactivated.\nPlease inquire at the library administrator.",
                                    "Account Deactivated",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );
                                this.Close();
                                return;
                            }

                            lblStudentNo.Text = reader["StudentNo"].ToString();
                            lblName.Text = reader["FirstName"].ToString() + " " + reader["LastName"].ToString();
                            lblCourse.Text = reader["Course"].ToString();
                            lblYearLevel.Text = reader["YearLevel"].ToString();
                        }
                    }
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            LoadControl(new AvailbleCopies { StudentNo = _studentNo });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.Close(); // ✅ Just close the StudentForm
            }
        }



        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
