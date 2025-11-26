using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Library_Management_System.User_Control_Student;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Forms
{
    public partial class StudentForm : Form
    {
        private string _studentNo;

        // ✅ Right-side indicator
        private Panel sidebarIndicator;
        private Button activeButton;
        private Timer moveTimer;
        private int targetTop;



        public StudentForm(string studentNo)
        {
            InitializeComponent();
            _studentNo = studentNo;

            LoadStudentInfo();

            // Initialize indicator panel
            sidebarIndicator = new Panel
            {
                Size = new Size(4, 40), // width 4px, height matches button
                BackColor = Color.FromArgb(242, 229, 217),
                Visible = false
            };
            panel1.Controls.Add(sidebarIndicator); // add to your sidebar panel
            sidebarIndicator.BringToFront();

            moveTimer = new Timer { Interval = 15 };
            moveTimer.Tick += MoveIndicatorSmooth;

            // Load AvailableCopies on startup
            AvailbleCopies availableCopiesUC = new AvailbleCopies
            {
                StudentNo = _studentNo,
                Dock = DockStyle.Fill
            };
            panelContent.Controls.Clear();
            panelContent.Controls.Add(availableCopiesUC);

            // Pre-select Book Hub after form loads
            this.Shown += (s, e) => PreSelectBookHub();
        }

        private void StartDueCheckTimer()
        {
            Timer timer = new Timer();
            timer.Interval = 1000 * 60 * 30; // every 30 mins
            timer.Tick += (s, e) => CheckDueSoonNotifications();
            timer.Start();
        }

        private void LoadControl(UserControl control)
        {
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Clear();
            panelContent.Controls.Add(control);
        }

        private void StudentForm_Load(object sender, EventArgs e)
        {
            StartDueCheckTimer();
            CheckDueSoonNotifications(); // initial check
            this.WindowState = FormWindowState.Maximized;
            LoadControl(new AvailbleCopies { StudentNo = _studentNo });
        }
        private void CheckDueSoonNotifications()
        {
            using (var con = new SQLiteConnection(Db.ConnectionString))
            {
                con.Open();

                string query = @"
            SELECT br.BorrowId, br.MemberId, br.DueDate, m.StudentNo, br.DueReminderSent, br.OverdueNotificationSent
            FROM Borrowings br
            INNER JOIN Members m ON br.MemberId = m.MemberId
            WHERE br.Status = 'Borrowed'
        ";

                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int borrowId = Convert.ToInt32(reader["BorrowId"]);
                        string studentNo = reader["StudentNo"].ToString();
                        DateTime dueDate = DateTime.Parse(reader["DueDate"].ToString());
                        bool dueReminderSent = reader["DueReminderSent"] != DBNull.Value && Convert.ToInt32(reader["DueReminderSent"]) == 1;
                        bool overdueNotificationSent = reader["OverdueNotificationSent"] != DBNull.Value && Convert.ToInt32(reader["OverdueNotificationSent"]) == 1;

                        // --- 1️⃣ Due Reminder ---
                        DateTime notifyTime = dueDate.AddDays(-1).Date.AddHours(18);
                        if (!dueReminderSent && DateTime.Now >= notifyTime && DateTime.Now < dueDate)
                        {
                            InsertNotificationDirect(con, studentNo,
                                $"Reminder: Your borrowed book is due tomorrow at {dueDate:hh:mm tt}.");

                            string updateQuery = "UPDATE Borrowings SET DueReminderSent = 1 WHERE BorrowId = @borrowId";
                            using (var updateCmd = new SQLiteCommand(updateQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }

                        // --- 2️⃣ Overdue Notification ---
                        DateTime penaltyStart = dueDate.AddHours(1);
                        if (!overdueNotificationSent && DateTime.Now >= penaltyStart)
                        {
                            InsertNotificationDirect(con, studentNo,
                                $"⚠️ Your borrowed book was due at {dueDate:hh:mm tt}. Penalty has now started.");

                            string updateQuery = "UPDATE Borrowings SET OverdueNotificationSent = 1 WHERE BorrowId = @borrowId";
                            using (var updateCmd = new SQLiteCommand(updateQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@borrowId", borrowId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        // Use this method instead of opening a new connection
        private void InsertNotificationDirect(SQLiteConnection con, string studentNo, string message)
        {
            string query = @"
        INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead, IsFollowUp)
        VALUES (@StudentNo, @Message, @DateCreated, 0, 0)
    ";

            using (var cmd = new SQLiteCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@StudentNo", studentNo);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }


        private void InsertNotification(string studentNo, string message)
        {
            using (var con = new SQLiteConnection(Db.ConnectionString))
            {
                con.Open();

                string query = @"
            INSERT INTO Notifications (StudentNo, Message, DateCreated, IsRead, IsFollowUp)
            VALUES (@StudentNo, @Message, @DateCreated, 0, 0)
        ";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@StudentNo", studentNo);
                    cmd.Parameters.AddWithValue("@Message", message);
                    cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void btnAvailableCopies_Click(object sender, EventArgs e)
        {
            SetActiveButton(btnAvailableCopies);
            LoadControl(new AvailbleCopies { StudentNo = _studentNo });
        }

        private void btnBorrowing_Click(object sender, EventArgs e)
        {
            SetActiveButton(btnBorrowing);
            LoadControl(new Borrowing { StudentNo = _studentNo });
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SetActiveButton(btnAvailableCopies);
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
                this.Close();
            }
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

        // ===================== RIGHT-SIDE INDICATOR FUNCTIONS =====================
        private void PreSelectBookHub()
        {
            SetActiveButton(btnAvailableCopies);
        }

        private void SetActiveButton(Button clickedButton)
        {
            // Reset previous button
            if (activeButton != null)
            {
                activeButton.BackColor = Color.Transparent;
                activeButton.ForeColor = Color.Black;
            }

            activeButton = clickedButton;
            activeButton.BackColor = Color.Transparent;
            activeButton.ForeColor = Color.FromArgb(242, 229, 217); // highlight

            // Position indicator
            sidebarIndicator.Height = activeButton.Height;

            int radius = 30; // match panel1's top-right corner
            sidebarIndicator.Left = panel1.Width - sidebarIndicator.Width - radius / 2; // adjust for rounded corner
            sidebarIndicator.Visible = true;
            sidebarIndicator.BringToFront();

            targetTop = activeButton.Top;
            moveTimer.Start();
        }


        private void MoveIndicatorSmooth(object sender, EventArgs e)
        {
            int currentTop = sidebarIndicator.Top;
            int diff = targetTop - currentTop;

            if (Math.Abs(diff) < 1)
            {
                sidebarIndicator.Top = targetTop;
                moveTimer.Stop();
                return;
            }

            double easingFactor = 0.15; // smooth animation
            sidebarIndicator.Top = (int)(currentTop + diff * easingFactor);
        }

        private void panelContent_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            int radius = 20; // adjust roundness

            Panel p = sender as Panel;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();

            // Top-left corner (sharp)
            path.AddLine(0, 0, p.Width - radius, 0);

            // Top-right corner (rounded)
            path.AddArc(p.Width - radius, 0, radius, radius, 270, 90);

            // Right side
            path.AddLine(p.Width, radius, p.Width, p.Height);

            // Bottom
            path.AddLine(p.Width, p.Height, 0, p.Height);

            // Left side
            path.AddLine(0, p.Height, 0, 0);

            path.CloseFigure();

            p.Region = new Region(path);
        }
    }
}
