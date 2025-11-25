using System;
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

        private void LoadControl(UserControl control)
        {
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Clear();
            panelContent.Controls.Add(control);
        }

        private void StudentForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            LoadControl(new AvailbleCopies { StudentNo = _studentNo });
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
