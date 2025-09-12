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
using LibraryManagementSystem.Data;

namespace Library_Management_System
{
    public partial class MainForm : Form
    {
        // Add this property here
        public bool IsAdmin { get; set; } = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            BooksForm booksForm = new BooksForm();
            booksForm.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MembersForm membersForm = new MembersForm();
            membersForm.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
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
