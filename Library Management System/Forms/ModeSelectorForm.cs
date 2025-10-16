using System;
using System.Windows.Forms;
using Library_Management_System.Forms;

namespace Library_Management_System
{
    public partial class ModeSelectorForm : Form
    {
        private PreLoginButtons preLoginForm;

        // ✅ Constructor now receives the PreLoginButtons reference
        public ModeSelectorForm(PreLoginButtons preLoginForm)
        {
            InitializeComponent();
            this.preLoginForm = preLoginForm; // store the reference

            this.StartPosition = FormStartPosition.CenterParent; // Center modal
            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#f7ebdd");
        }

        private void ModeSelectorForm_Load(object sender, EventArgs e)
        {
        }

        private void btnStudent_Click_1(object sender, EventArgs e)
        {
            // ✅ Hide the PreLoginButtons form
            preLoginForm.Hide();

            // Open LoginForm
            LoginForm loginForm = new LoginForm();

            // Optional: show PreLoginButtons again after LoginForm closes
            loginForm.FormClosed += (s, args) => preLoginForm.Show();

            loginForm.Show();

            // Close ModeSelectorForm
            this.Close();
        }

        private void btnAdmin_Click_1(object sender, EventArgs e)
        {
            preLoginForm.Hide();

            AdminForm adminForm = new AdminForm();
            //adminForm.FormClosed += (s, args) => preLoginForm.Show(); // optional
            adminForm.Show();

            this.Close();
        }

        private void btnRegister_Click_1(object sender, EventArgs e)
        {
            preLoginForm.Hide();

            RegisterForm registerForm = new RegisterForm();
            registerForm.FormClosed += (s, args) => preLoginForm.Show();

            registerForm.Show();

            this.Close();
        }
    }
}
