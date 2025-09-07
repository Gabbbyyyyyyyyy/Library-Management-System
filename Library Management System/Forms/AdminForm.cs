using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryManagementSystem;

namespace Library_Management_System.Forms
{
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();

            // Disable password textbox initially
            txtPassword.Enabled = false;

            // Listen for changes in username textbox
            txtUsername.TextChanged += TxtUsername_TextChanged;

        }

        private void TxtUsername_TextChanged(object sender, EventArgs e)
        {
            // Enable password textbox only if username is not empty
            if (!string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                txtPassword.Enabled = true;
            }
            else
            {
                txtPassword.Enabled = false;
            }
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // --- Check if username is empty first ---
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter your username first.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return; // Stop here
            }
            // --- Check if password is empty ---
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return; // Stop here
            }


            // --- Admin Login ---
            if (username == "admin" && password == "admin")
            {
                MessageBox.Show("Login successful!", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.ShowDialog();
                this.Close();
                return;
            }

            // --- Incorrect credentials ---
            MessageBox.Show("Incorrect username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.Clear(); // Clear password for security
            txtPassword.Focus(); // Focus back on password
            }

        }
    }

