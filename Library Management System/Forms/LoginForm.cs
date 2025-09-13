using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
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
            Db.EnsureCreated(); // Create DB if not exist
        }

        private void lblMessage_Click(object sender, EventArgs e)
        {

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
    }
}