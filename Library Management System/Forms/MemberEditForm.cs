using System;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem
{
    public partial class MemberEditForm : Form
    {
        private int memberId;

        public MemberEditForm(int id, string firstName, string lastName, string course, string yearLevel)
        {
            InitializeComponent();
            memberId = id;
            txtFirstName.Text = firstName;
            txtLastName.Text = lastName;
            txtCourse.Text = course;
            txtYearLevel.Text = yearLevel;
        }

        // First Name (letters only, not blank)
        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("First Name cannot be blank.");
             
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtFirstName.Text, @"^[a-zA-Z\s]*$"))
            {
                MessageBox.Show("First Name must contain words only (no numbers or special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtFirstName.Text = new string(txtFirstName.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtFirstName.SelectionStart = txtFirstName.Text.Length;
            }
        }

        // Last Name (letters only, not blank)
        private void txtLastName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Last Name cannot be blank.");
               
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtLastName.Text, @"^[a-zA-Z\s]*$"))
            {
                MessageBox.Show("Last Name must contain words only (no numbers or special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtLastName.Text = new string(txtLastName.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtLastName.SelectionStart = txtLastName.Text.Length;
            }
        }

        // Course (letters only, not blank)
        private void txtCourse_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourse.Text))
            {
                MessageBox.Show("Course cannot be blank.");
                
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtCourse.Text, @"^[a-zA-Z\s]*$"))
            {
                MessageBox.Show("Course must contain words only (no numbers or special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtCourse.Text = new string(txtCourse.Text.Where(c => char.IsLetter(c) || char.IsWhiteSpace(c)).ToArray());
                txtCourse.SelectionStart = txtCourse.Text.Length;
            }
        }

        // Year Level (numbers only, 1–6)
        private void txtYearLevel_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtYearLevel.Text))
            {
                MessageBox.Show("Year Level cannot be blank.");
                
            }

            // 🔹 Detect if input contains letters
            if (txtYearLevel.Text.Any(char.IsLetter))
            {
                MessageBox.Show("Year Level cannot contain letters or words. Please enter numbers only.",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Remove all letters
                txtYearLevel.Text = new string(txtYearLevel.Text.Where(char.IsDigit).ToArray());
                txtYearLevel.SelectionStart = txtYearLevel.Text.Length;
                return;
            }

            // 🔹 Keep only digits
            txtYearLevel.Text = new string(txtYearLevel.Text.Where(char.IsDigit).ToArray());
            txtYearLevel.SelectionStart = txtYearLevel.Text.Length;

            if (int.TryParse(txtYearLevel.Text, out int year))
            {
                if (year < 1 || year > 6)
                {
                    MessageBox.Show("Year Level must be between 1 and 6.");
                    
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "UPDATE Members SET FirstName=@fn, LastName=@ln, Course=@course, YearLevel=@yl WHERE MemberId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@fn", txtFirstName.Text);
                    cmd.Parameters.AddWithValue("@ln", txtLastName.Text);
                    cmd.Parameters.AddWithValue("@course", txtCourse.Text);
                    cmd.Parameters.AddWithValue("@yl", txtYearLevel.Text);
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member updated successfully!");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void MemberEditForm_Load(object sender, EventArgs e)
        {

        }
    }
}
