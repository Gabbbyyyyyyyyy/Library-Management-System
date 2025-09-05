using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using LibraryManagementSystem.Data;
using System.Data.SQLite;
using System.Security.Cryptography;

// put validation in registration form to not the same StudentNO.

namespace Library_Management_System.Forms
{
    public partial class RegisterForm : Form
    {
      
        public RegisterForm()
        {
            InitializeComponent();
        }

        // Last Name (letters only, not blank)
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
                MessageBox.Show("Last Name must contain words only (no numbers or special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtLastName.Text = new string(txtLastName.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtLastName.SelectionStart = txtLastName.Text.Length;
            }

            lblMessage.Text = "";
        }

        private void btnRegisterUser_Click(object sender, EventArgs e)
        {
            string studentNo = txtStudentNo.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string course = txtCourse.Text.Trim();
            string yearLevel = textYearLevel.Text.Trim();

            // 🔹 Check if any field is blank
            if (string.IsNullOrWhiteSpace(studentNo) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(course) ||
                string.IsNullOrWhiteSpace(yearLevel))
            {
                MessageBox.Show("All fields must be filled.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate Student No
            if (!System.Text.RegularExpressions.Regex.IsMatch(studentNo, @"^\d{6}$"))
            {
                MessageBox.Show("Student No must be exactly 6 digits (numbers only).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate First Name
            if (!System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("First Name must contain words only (no numbers or special characters).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate Last Name
            if (!System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Last Name must contain words only (no numbers or special characters).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate Course
            if (!System.Text.RegularExpressions.Regex.IsMatch(course, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Course must contain words only (no numbers or special characters).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validate Year Level
            if (!int.TryParse(yearLevel, out int year) || year < 1 || year > 6)
            {
                MessageBox.Show("Year Level must be a number between 1 and 6.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();

                // 🔹 Check if Student No already exists
                string checkQuery = "SELECT COUNT(1) FROM Members WHERE StudentNo=@studentNo";
                SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@studentNo", studentNo);

                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (exists == 1)
                {
                    MessageBox.Show("Student No already exists!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 🔹 Insert into Members table
                string insertQuery = @"INSERT INTO Members 
            (StudentNo, FirstName, LastName, Course, YearLevel) 
            VALUES (@studentNo, @firstName, @lastName, @course, @yearLevel)";
                SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, con);
                insertCmd.Parameters.AddWithValue("@studentNo", studentNo);
                insertCmd.Parameters.AddWithValue("@firstName", firstName);
                insertCmd.Parameters.AddWithValue("@lastName", lastName);
                insertCmd.Parameters.AddWithValue("@course", course);
                insertCmd.Parameters.AddWithValue("@yearLevel", yearLevel);
                insertCmd.ExecuteNonQuery();
            }

            MessageBox.Show("Registration successful! Please login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // 🔹 Open login form
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();
        }



        // Student Number (exactly 6 digits, not blank)
        private void txtStudentNo_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentNo.Text))
            {
                lblMessage.Text = "Student No cannot be blank.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtStudentNo.Text, @"^\d{0,6}$"))
            {
                txtStudentNo.Text = new string(txtStudentNo.Text.Where(char.IsDigit).ToArray());
                txtStudentNo.SelectionStart = txtStudentNo.Text.Length;
            }

            if (txtStudentNo.Text.Length != 6)
            {
                lblMessage.Text = "Student No must be exactly 6 digits.";
                lblMessage.ForeColor = Color.Red;
            }
            else
            {
                lblMessage.Text = "";
            }
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        // First Name (letters only, not blank)
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
                MessageBox.Show("First Name must contain words only (no numbers or special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtFirstName.Text = new string(txtFirstName.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtFirstName.SelectionStart = txtFirstName.Text.Length;
            }

            lblMessage.Text = "";
        }

        // Course (letters only, not blank)
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
                MessageBox.Show("Course must contain words only (no numbers or special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtCourse.Text = new string(txtCourse.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtCourse.SelectionStart = txtCourse.Text.Length;
            }

            lblMessage.Text = "";
        }


        // Year Level (numbers only, 1–6)
        private void textYearLevel_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textYearLevel.Text))
            {
                lblMessage.Text = "Year Level cannot be blank.";
                lblMessage.ForeColor = Color.Red;
                return;
            }

            // 🔹 Detect if input contains letters
            if (textYearLevel.Text.Any(char.IsLetter))
            {
                MessageBox.Show("Year Level cannot contain letters or words. Please enter numbers only.",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Remove all letters
                textYearLevel.Text = new string(textYearLevel.Text.Where(char.IsDigit).ToArray());
                textYearLevel.SelectionStart = textYearLevel.Text.Length;
                return;
            }

            // 🔹 Keep only digits
            textYearLevel.Text = new string(textYearLevel.Text.Where(char.IsDigit).ToArray());
            textYearLevel.SelectionStart = textYearLevel.Text.Length;

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
                }
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
