using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Library_Management_System.User_Control_Student;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Forms
{
    public partial class StudentForm : Form
    {
        private string _studentNo;
        public static event Action BookReturned;

        public StudentForm(string studentNo)
        {
            InitializeComponent();
            _studentNo = studentNo;
            LoadStudentInfo();
            this.Load += StudentForm_Load;

        }

        private void LoadStudentInfo()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT StudentNo, FirstName, LastName, Course, YearLevel, IsActive FROM Members WHERE StudentNo=@studentNo";


                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", _studentNo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            // Check if account is active
                            bool isActive = Convert.ToInt32(reader["IsActive"]) == 1;
                            if (!isActive)
                            {
                                MessageBox.Show("⚠️ Your account has been deactivated.\n" +
                                                    "Please inquire at the library administrator.",
                                                    "Account Deactivated",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Warning
                                                );

                                this.Close(); // close the StudentForm
                                return;
                            }
                            // If active → show student info
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
            CheckBookNotifications();
            

        }
        /// <summary>
        /// Checks borrowed books for the logged-in student and updates the notification panel
        /// </summary>
        /// 
       

       


        private void CheckBookNotifications()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"SELECT br.BorrowId, b.Title, br.DueDate, br.ReturnDate
                 FROM Borrowing br
                 INNER JOIN Books b ON b.BookId = br.BookId
                 INNER JOIN Members m ON m.MemberId = br.MemberId
                 WHERE m.StudentNo = @studentNo AND br.ReturnDate IS NULL";


                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", _studentNo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        string notifications = "";

                        while (reader.Read())
                        {
                            int borrowId = Convert.ToInt32(reader["BorrowId"]);
                            string title = reader["Title"].ToString();
                            DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);

                            // Current time
                            DateTime now = DateTime.Now;

                            // Case 1: Overdue (after 8:01 AM next day)
                            if (now > dueDate)
                            {
                                int daysLate = (now.Date - dueDate.Date).Days;
                                double penalty = 100; // base penalty
                                if (daysLate > 1)
                                    penalty += (daysLate - 1) * 2; // +2 per extra day

                                notifications += $"⚠️ \"{title}\" is overdue! Current Penalty: ₱{penalty}.\n";

                                // Update penalty in DB
                                using (var updateCmd = new SQLiteCommand("UPDATE BorrowedBooks SET Penalty = @penalty WHERE BorrowId = @borrowId", con))
                                {
                                    updateCmd.Parameters.AddWithValue("@penalty", penalty);
                                    updateCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            // Case 2: Reminder at 9:00 PM same day
                            else if (now.Date == dueDate.Date.AddDays(-1) && now.Hour >= 21)
                            {
                                notifications += $"ℹ️ Reminder: \"{title}\" is due tomorrow at 8:00 AM. Please return on time.\n";
                            }
                            // Case 3: Reminder at 7:00 AM next day
                            else if (now.Date == dueDate.Date && now.Hour == 7)
                            {
                                notifications += $"⏰ Final Reminder: \"{title}\" is due in 1 hour (8:00 AM).\n";
                            }
                            // Case 4: General due soon (before 8 AM same day)
                            else if (now.Date == dueDate.Date && now < dueDate)
                            {
                                notifications += $"ℹ️ \"{title}\" is due today at 8:00 AM. Return on time to avoid penalty.\n";
                            }
                        }

                        
                    }
                }
            }
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void lblNotification_Click(object sender, EventArgs e)
        {
            
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Show message first
            MessageBox.Show("Logout successful!", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Now close the StudentForm → LoginForm will reappear (because of FormClosed handler in LoginForm)
            this.Close();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvAvailableBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
       


        private void btnAvailableCopies_Click(object sender, EventArgs e)
        {
          
        }

        private void dgvAvailableBooks_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvAvailableBooks_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvBorrowedBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabControlStudent_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabControlStudent_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void panel1_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void btnAvailableCopies_Click_1(object sender, EventArgs e)
        {
           
        }

        

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
