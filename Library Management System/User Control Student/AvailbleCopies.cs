using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace Library_Management_System.User_Control_Student
{
    public partial class AvailbleCopies : UserControl
    {
        public AvailbleCopies()
        {
            InitializeComponent(); // Calls designer-generated method
            LoadAvailableCopies(); // Your custom method
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void LoadAvailableCopies()
        {
            try
            {
                using (var con = Db.GetConnection())
                {
                    con.Open();
                    string query = @"
                SELECT BookId, Title, Author, AvailableCopies
                FROM Books
                WHERE AvailableCopies > 0";

                    using (var cmd = new SQLiteCommand(query, con))
                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dataGridView1.DataSource = dt;

                        // Optional: set column headers
                        dataGridView1.Columns["BookId"].HeaderText = "ID";
                        dataGridView1.Columns["Title"].HeaderText = "Title";
                        dataGridView1.Columns["Author"].HeaderText = "Author";
                        dataGridView1.Columns["AvailableCopies"].HeaderText = "Available Copies";

                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                        dataGridView1.RowTemplate.Height = 30;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading available copies: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
