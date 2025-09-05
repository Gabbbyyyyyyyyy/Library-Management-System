using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Library_Management_System.Forms
{
    public partial class PreLoginButtons : Form
    {
        public PreLoginButtons()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            AdminForm adminForm = new AdminForm();
            adminForm.FormClosed += (s, args) => this.Show(); // Show this form again when MainForm is closed
            adminForm.Show();
        }

        private void btnStudent_Click(object sender, EventArgs e)
        {
            this.Hide(); // Hide the PreLoginButtons form
            LoginForm loginForm = new LoginForm();
            loginForm.FormClosed += (s, args) => this.Show(); // Show this form again when LoginForm is closed
            loginForm.Show();
        }

        private void PreLoginButtons_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            RegisterForm registerForm = new RegisterForm();
            registerForm.ShowDialog();
        }
    }
}
