using System;
using System.Data.SQLite;
using System.Drawing;
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

                string query = @"SELECT bb.BorrowId, b.Title, bb.DueDate, bb.ReturnDate
                         FROM BorrowedBooks bb
                         INNER JOIN Books b ON b.BookId = bb.BookId
                         INNER JOIN Members m ON m.MemberId = bb.MemberId
                         WHERE m.StudentNo = @studentNo AND bb.ReturnDate IS NULL";

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

                            if (DateTime.Now.Date > dueDate.Date) // overdue
                            {
                                int daysLate = (DateTime.Now.Date - dueDate.Date).Days;

                                // Apply penalty rule
                                double penalty = 100; // base penalty
                                if (daysLate > 1)
                                {
                                    penalty += (daysLate - 1) * 2; // +2 per extra day
                                }
                                notifications += $"⚠️ \"{title}\" is overdue by {daysLate} day(s)! Current Penalty: ₱{penalty}.\n";

                                // Update penalty in DB
                                using (var updateCmd = new SQLiteCommand("UPDATE BorrowedBooks SET Penalty = @penalty WHERE BorrowId = @borrowId", con))
                                {
                                    updateCmd.Parameters.AddWithValue("@penalty", penalty);
                                    updateCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else if ((dueDate.Date - DateTime.Now.Date).TotalDays <= 1) // due soon
                            {
                                notifications += $"ℹ️ \"{title}\" is due on {dueDate:MMMM dd, yyyy}. Return on time to avoid a ₱100 penalty.\n";
                            }
                        }

                        // Show or hide notification panel
                        if (!string.IsNullOrEmpty(notifications))
                        {
                            panelNotification.Visible = true;
                            lblNotification.Text = notifications;
                        }
                        else
                        {
                            panelNotification.Visible = false;
                            lblNotification.Text = "";
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
    }
}
