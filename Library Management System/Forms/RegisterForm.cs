using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();

            // Center modal form
            this.StartPosition = FormStartPosition.CenterScreen;

            // Fill screen
            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            // Enable smoother drawing
            this.DoubleBuffered = true;
            this.Paint += RegisterForm_Paint;

            // Handle resize to keep panel and logo centered
            this.Resize += RegisterForm_Resize;

            btnRegisterUser.BackColor = ColorTranslator.FromHtml("#381313");
            btnRegisterUser.ForeColor = Color.White;
            btnRegisterUser.FlatStyle = FlatStyle.Flat;


            //// Apply rounded edges
            SetButtonRounded(btnRegisterUser, 1); // 20 = corner radius

        }

        private void SetButtonRounded(Button btn, int radius)
        {
            // Make sure system doesn't draw its own border
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TabStop = false; // prevents focus dotted rectangle (optional)

            // Recreate region whenever size changes
            btn.Resize -= Btn_Resize; // avoid duplicate handlers
            btn.Resize += Btn_Resize;

            // Paint custom border
            btn.Paint -= Btn_Paint;
            btn.Paint += Btn_Paint;

            // Apply initial region now
            ApplyRoundedRegion(btn, radius);
        }

        private void Btn_Resize(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
                ApplyRoundedRegion(btn, 20); // or store radius per-button if needed
        }

        private void ApplyRoundedRegion(Button btn, int radius)
        {
            Rectangle rect = btn.ClientRectangle;
            int diameter = radius * 2;

            using (GraphicsPath path = new GraphicsPath())
            {
                // Use RectangleF to be precise and avoid clipping
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                // Set region from path (region will clip the control shape perfectly)
                btn.Region = new Region(path);
            }
        }

        private void Btn_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            int radius = 20;                 // same radius as before
            float penWidth = 1.4f;           // slightly thicker for smoother edges
            Color borderColor = Color.FromArgb(120, 56, 19, 19); // rich but soft brown

            // Enable highest-quality smoothing
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Draw slightly inside the button bounds
            RectangleF rect = new RectangleF(
                penWidth / 2f,
                penWidth / 2f,
                btn.ClientSize.Width - penWidth - 1f,
                btn.ClientSize.Height - penWidth - 1f
            );

            float diameter = radius * 2f;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                using (Pen pen = new Pen(borderColor, penWidth))
                {
                    pen.LineJoin = LineJoin.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    e.Graphics.DrawPath(pen, path);
                }
            }
        }




        private void RegisterForm_Load(object sender, EventArgs e)
        {
            CenterPanelAndLogo();

        }

        private void RegisterForm_Resize(object sender, EventArgs e)
        {
            CenterPanelAndLogo();
        }

        private void CenterPanelAndLogo()
        {
            // Adjust panel width by 2% of the form's width
            int newWidth = (int)(this.ClientSize.Width * 0.02) + panel1.Width;
            panel1.Width = newWidth;
            // Center panel
            panel1.Left = (this.ClientSize.Width - panel1.Width) / 2;
            panel1.Top = (this.ClientSize.Height - panel1.Height) / 2;

            // Center logo above panel with a reduced offset
            if (pictureBox1.Image != null)
            {
                pictureBox1.Left = panel1.Left + (panel1.Width - pictureBox1.Width) / 2;
                pictureBox1.Top = panel1.Top - pictureBox1.Height;  // Position logo directly above the panel without overlap
                pictureBox1.BringToFront();
            }
        }



        private void RegisterForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int shadowSize = 15;
            Color shadowColor = Color.FromArgb(50, 0, 0, 0);

            Rectangle rect = panel1.Bounds;

            for (int i = shadowSize; i >= 1; i--)
            {
                int alpha = (int)(shadowColor.A * (1f - (i / (float)(shadowSize + 2))));
                using (Pen pen = new Pen(Color.FromArgb(alpha, shadowColor), 1))
                {
                    g.DrawRectangle(pen, new Rectangle(
                        rect.Left - i,
                        rect.Top - i,
                        rect.Width + i * 2,
                        rect.Height + i * 2));
                }
            }
        }

        // ===== Validation Handlers =====
        private void txtLastName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                lblMessage.Text = "Last Name cannot be blank.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtLastName.Text, @"^[a-zA-Z\s]*$"))
            {
                MessageBox.Show("Last Name must contain words only.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Text = new string(txtLastName.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtLastName.SelectionStart = txtLastName.Text.Length;
            }

            lblMessage.Text = "";
        }

        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                lblMessage.Text = "First Name cannot be blank.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtFirstName.Text, @"^[a-zA-Z\s]*$"))
            {
                MessageBox.Show("First Name must contain words only.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Text = new string(txtFirstName.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtFirstName.SelectionStart = txtFirstName.Text.Length;
            }

            lblMessage.Text = "";
        }

        private void txtCourse_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourse.Text))
            {
                lblMessage.Text = "Course cannot be blank.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtCourse.Text, @"^[a-zA-Z\s]*$"))
            {
                MessageBox.Show("Course must contain words only.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCourse.Text = new string(txtCourse.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtCourse.SelectionStart = txtCourse.Text.Length;
            }

            lblMessage.Text = "";
        }

        private void textYearLevel_TextChanged(object sender, EventArgs e)
        {
            string input = textYearLevel.Text;

            // If user types letters
            if (input.Any(c => !char.IsDigit(c)))
            {
                lblMessage.Text = "Year Level can't contain letters.";
                lblMessage.ForeColor = Color.Red;
                textYearLevel.Text = new string(input.Where(char.IsDigit).ToArray());
                textYearLevel.SelectionStart = textYearLevel.Text.Length;
                return;
            }

            // Limit input to 1 character only
            if (textYearLevel.Text.Length > 1)
            {
                lblMessage.Text = "Year Level can only be a single digit (1-6).";
                lblMessage.ForeColor = Color.Red;
                textYearLevel.Text = textYearLevel.Text.Substring(0, 1);
                textYearLevel.SelectionStart = textYearLevel.Text.Length;
                return;
            }

            // Check if the number is between 1 and 6
            if (int.TryParse(textYearLevel.Text, out int year))
            {
                if (year < 1 || year > 6)
                {
                    lblMessage.Text = "Year Level must be between 1 and 6.";
                    lblMessage.ForeColor = Color.Red;
                }
                else
                {
                    lblMessage.Text = "";
                    lblMessage.ForeColor = Color.Black;
                }
            }
        }



        private void txtStudentNo_TextChanged(object sender, EventArgs e)
        {
            string input = txtStudentNo.Text;

            // If user types letters
            if (input.Any(c => !char.IsDigit(c)))
            {
                lblMessage.Text = "Student No can't contain letters.";
                lblMessage.ForeColor = Color.Red;

                // Remove letters
                txtStudentNo.Text = new string(input.Where(char.IsDigit).ToArray());
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length;
                return;
            }

            // Limit input to 6 digits
            if (txtStudentNo.Text.Length > 6)
            {
                txtStudentNo.Text = txtStudentNo.Text.Substring(0, 6);
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length;
            }

            // Clear message if input is valid
            if (!string.IsNullOrEmpty(txtStudentNo.Text))
            {
                lblMessage.Text = "";
                lblMessage.ForeColor = Color.Black;
            }
            else
            {
                lblMessage.Text = "Student No can't contain letters.";
                lblMessage.ForeColor = Color.Red;
            }
        }


        private void btnRegisterUser_Click(object sender, EventArgs e)
        {
            string studentNo = txtStudentNo.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string course = txtCourse.Text.Trim();
            string yearLevel = textYearLevel.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentNo) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(course) ||
                string.IsNullOrWhiteSpace(yearLevel))
            {
                MessageBox.Show("All fields must be filled.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();

                string checkQuery = "SELECT COUNT(1) FROM Members WHERE StudentNo=@studentNo";
                SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@studentNo", studentNo);

                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (exists == 1)
                {
                    MessageBox.Show("Student No already exists!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Add DateJoined column to the insert query
                string insertQuery = @"
            INSERT INTO Members (StudentNo, FirstName, LastName, Course, YearLevel, DateJoined, IsActive)
            VALUES (@studentNo, @firstName, @lastName, @course, @yearLevel, @dateJoined, 1);
        ";

                SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@studentNo", studentNo);
                insertCmd.Parameters.AddWithValue("@firstName", firstName);
                insertCmd.Parameters.AddWithValue("@lastName", lastName);
                insertCmd.Parameters.AddWithValue("@course", course);
                insertCmd.Parameters.AddWithValue("@yearLevel", yearLevel);

                // 👇 Add DateJoined value here
                insertCmd.Parameters.AddWithValue("@dateJoined", DateTime.Now.ToString("yyyy-MM-dd"));

                insertCmd.ExecuteNonQuery();
            }

            MessageBox.Show("Registration successful! Please login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            new LoginForm().Show();
            this.Close();
        }


        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
