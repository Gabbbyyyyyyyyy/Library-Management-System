using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Library_Management_System.User_Control;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;

namespace Library_Management_System
{
    public partial class MainForm : Form
    {
        // Add this property here
        public bool IsAdmin { get; set; } = false;

        // Inside MainForm.Designer.cs
        private System.Windows.Forms.PictureBox pictureBoxLogo;


        public MainForm()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();



            // MainForm
            this.Controls.Add(this.pictureBoxLogo);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);

        }



        private void button1_Click(object sender, EventArgs e)
        {
            // New way (loads UserControl into panelContainer):
            LoadControl(new ManageBooksControl());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadControl(new ManageMembersControl());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            LoadControl(new DashboardControl());
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Show message first
            MessageBox.Show("Logout successful!", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Now close the StudentForm → LoginForm will reappear (because of FormClosed handler in LoginForm)
            this.Close();
        }

        private void panelContainer_Paint(object sender, PaintEventArgs e)
        {

        }
        private void LoadControl(UserControl control)
        {
            panelContainer.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelContainer.Controls.Add(control);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }


        //private void button1_Click(object sender, EventArgs e)
        //{
        //    using (var con = Db.GetConnection())
        //    {
        //        con.Open();
        //        MessageBox.Show("Database connection successful!");
        //    }
        //}
    }
}
