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
            this.StartPosition = FormStartPosition.CenterScreen;
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

            if (!System.Text.RegularExpressions.Regex.IsMatch(txtFirstName.Text, @"^[a-zA-Z\s.]*$"))
            {
                MessageBox.Show("First Name must contain letters, spaces, or periods only (no numbers or other special characters).",
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Keep only letters, spaces, or periods
                txtFirstName.Text = new string(txtFirstName.Text
                                               .Where(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '.')
                                               .ToArray());

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
            string input = txtYearLevel.Text;

            // Limit input to 2 characters
            if (input.Length > 2)
            {
                MessageBox.Show("Year Level can only be up to 2 digits.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearLevel.Text = input.Substring(0, 2);
                txtYearLevel.SelectionStart = txtYearLevel.Text.Length;
                return;
            }

            // Remove any non-digit input
            if (input.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Year Level cannot contain letters. Please enter numbers only.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearLevel.Text = new string(input.Where(char.IsDigit).ToArray());
                txtYearLevel.SelectionStart = txtYearLevel.Text.Length;
                return;
            }

            // Validate numeric range
            if (int.TryParse(txtYearLevel.Text, out int year))
            {
                // Allow College: 1-6, Senior High: 11-12
                if (!((year >= 1 && year <= 6) || (year >= 11 && year <= 12)))
                {
                    MessageBox.Show("Year Level must be 1-6 (College) or 11-12 (Senior High).", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Trim all inputs
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string course = txtCourse.Text.Trim();
            string yearLevel = txtYearLevel.Text.Trim();

            // ✅ First Name validation
            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("First Name cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[a-zA-Z\s.]+$"))
            {
                MessageBox.Show("First Name must contain letters, spaces, or periods only.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return;
            }

            // ✅ Last Name validation
            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Last Name cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Last Name must contain letters and spaces only.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return;
            }

            // ✅ Course validation
            if (string.IsNullOrWhiteSpace(course))
            {
                MessageBox.Show("Course cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCourse.Focus();
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(course, @"^[a-zA-Z\s]+$"))
            {
                MessageBox.Show("Course must contain letters and spaces only.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCourse.Focus();
                return;
            }

            // ✅ Year Level validation
            if (string.IsNullOrWhiteSpace(yearLevel))
            {
                MessageBox.Show("Year Level cannot be blank.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearLevel.Focus();
                return;
            }
            if (!int.TryParse(yearLevel, out int yl))
            {
                MessageBox.Show("Year Level must be a number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearLevel.Focus();
                return;
            }
            if (!((yl >= 1 && yl <= 6) || (yl >= 11 && yl <= 12)))
            {
                MessageBox.Show("Year Level must be 1–6 (College) or 11–12 (Senior High).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYearLevel.Focus();
                return;
            }

            // ✅ Course vs Year Level validation
            string courseUpper = course.ToUpper();
            if (yl >= 1 && yl <= 6)
            {
                // College: course must start with "BS"
                if (!courseUpper.StartsWith("BS"))
                {
                    MessageBox.Show("College courses must start with 'BS' (e.g., BS Computer Science).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCourse.Focus();
                    return;
                }
            }
            else if (yl >= 11 && yl <= 12)
            {
                // Senior High: only ABM, STEM, TVL allowed
                string[] allowedSHCourses = { "ABM", "STEM", "TVL" };
                if (!allowedSHCourses.Contains(courseUpper))
                {
                    MessageBox.Show("Senior High courses must be ABM, STEM, or TVL.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCourse.Focus();
                    return;
                }
            }

            // ✅ If all validations pass, save to database
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "UPDATE Members SET FirstName=@fn, LastName=@ln, Course=@course, YearLevel=@yl WHERE MemberId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@fn", firstName);
                    cmd.Parameters.AddWithValue("@ln", lastName);
                    cmd.Parameters.AddWithValue("@course", course);
                    cmd.Parameters.AddWithValue("@yl", yearLevel);
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Student updated successfully!");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void MemberEditForm_Load(object sender, EventArgs e)
        {

        }
    }
}
