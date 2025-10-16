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
        }

        private void lblMessage_Click(object sender, EventArgs e)
        {

        }
        private void LoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Call the button1_Click event
                btnLogin_Click(sender, e);

                // Prevent the "ding" sound
                e.SuppressKeyPress = true;
            }
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            string studentNo = txtStudentNo.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentNo))
            {
                lblMessage.Text = "Please enter Student No.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Check if student exists and is active
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
                        lblMessage.ForeColor = System.Drawing.Color.Red;
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
                    // When StudentForm is closed/logged out, reset login form
                    this.Show();
                    txtStudentNo.Clear();
                    lblMessage.Text = "";
                };
                studentForm.Show();
            }
            else
            {
                lblMessage.Text = "Invalid Student No.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                txtStudentNo.Clear();
            }
        }





        private void txtPassword_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            // Allow only digits
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtStudentNo.Text, @"^\d*$"))
            {
                MessageBox.Show("Only numbers are allowed.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentNo.Text = new string(txtStudentNo.Text.Where(char.IsDigit).ToArray()); // Remove non-digits
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length; // Keep cursor at end
                return;
            }
            // Limit to 6 digits
            if (txtStudentNo.Text.Length > 6)
            {
                MessageBox.Show("Student number must be exactly 6 digits.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStudentNo.Text = txtStudentNo.Text.Substring(0, 6);
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length; // Keep cursor at end
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Center the panel on the form
            panel1.Location = new Point(
                (this.ClientSize.Width - panel1.Width) / 2,
                (this.ClientSize.Height - panel1.Height) / 2
            );

            // Optional: make sure the panel repaints properly
            panel1.Invalidate();
        }



        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int shadowSize = 20; // how far the shadow spreads
            int maxAlpha = 120;  // shadow darkness (0–255)

            // Draw shadow outside the panel boundaries
            for (int i = 0; i < shadowSize; i++)
            {
                int alpha = maxAlpha - (i * maxAlpha / shadowSize);
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillRectangle(
                        brush,
                        -i,                     // expand left
                        -i,                     // expand top
                        panel.Width + i * 2,    // expand right
                        panel.Height + i * 2    // expand bottom
                    );
                }
            }

            // Draw the actual panel background over the shadow
            using (SolidBrush backBrush = new SolidBrush(panel.BackColor))
            {
                g.FillRectangle(backBrush, 0, 0, panel.Width, panel.Height);
            }
        }








    }
}