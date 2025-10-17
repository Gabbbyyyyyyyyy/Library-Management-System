using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Library_Management_System.Services;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Forms
{
    public partial class LoginForm : Form
    {
        // Declare it here
        private AuthService authService = new AuthService();

        public LoginForm()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;
            Db.EnsureCreated(); // Create DB if not exist
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(LoginForm_KeyDown);

            // Enable smoother drawing
            this.DoubleBuffered = true;
            this.Paint += LoginForm_Paint; // ✅ Draw shadow here instead of inside the panel
        }

        private void lblMessage_Click(object sender, EventArgs e) { }

        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string studentNo = txtStudentNo.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentNo))
            {
                lblMessage.Text = "Please enter Student No.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT IsActive FROM Members WHERE StudentNo = @studentNo";
                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@studentNo", studentNo);
                    object result = cmd.ExecuteScalar();

                    if (result == null)
                    {
                        lblMessage.Text = "Invalid Student No.";
                        lblMessage.ForeColor = Color.Red;
                        return;
                    }

                    int isActive = Convert.ToInt32(result);

                    if (isActive == 0)
                    {
                        MessageBox.Show(
                            "Your account has been deactivated.\nPlease inquire at the library admin desk.",
                            "Account Deactivated",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        txtStudentNo.Clear();
                        return;
                    }
                }
            }

            // ✅ Student is active, proceed to login
            if (authService.Authenticate(studentNo))
            {
                this.Hide();
                StudentForm studentForm = new StudentForm(studentNo);
                studentForm.FormClosed += (s, args) =>
                {
                    this.Show();
                    txtStudentNo.Clear();
                    lblMessage.Text = "";
                };
                studentForm.Show();
            }
            else
            {
                lblMessage.Text = "Invalid Student No.";
                lblMessage.ForeColor = Color.Red;
                txtStudentNo.Clear();
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            // Allow only digits
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtStudentNo.Text, @"^\d*$"))
            {
                MessageBox.Show("Only numbers are allowed.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentNo.Text = new string(txtStudentNo.Text.Where(char.IsDigit).ToArray());
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length;
                return;
            }

            // Limit to 6 digits
            if (txtStudentNo.Text.Length > 6)
            {
                MessageBox.Show("Student number must be exactly 6 digits.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentNo.Text = txtStudentNo.Text.Substring(0, 6);
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length;
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            //this.BackColor = System.Drawing.ColorTranslator.FromHtml("#f7ebdd");

            // Center the panel on the form
            panel1.Location = new Point(
                (this.ClientSize.Width - panel1.Width) / 2,
                (this.ClientSize.Height - panel1.Height) / 2
            );

            panel1.Invalidate();
        }

        // ✅ Draw shadow around panel1 here
        private void LoginForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int shadowSize = 15; // thicker
            Color shadowColor = Color.FromArgb(50, 0, 0, 0); // darker

            Rectangle rect = panel1.Bounds;

            // Draw shadow outward in layers
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
