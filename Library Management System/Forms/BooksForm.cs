using System;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem
{
    public partial class BooksForm : Form
    {
        public BooksForm()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks(string search = "")
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT BookId, ISBN, Title, Author, Category, " +
                               "CASE WHEN IsBorrowed = 1 THEN 'Borrowed' ELSE 'Available' END AS Status " +
                               "FROM Books";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE Title LIKE @search OR Author LIKE @search OR Category LIKE @search";
                }

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrWhiteSpace(search))
                        cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvBooks.DataSource = dt;
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "INSERT INTO Books (ISBN, Title, Author, Category, IsBorrowed) " +
                               "VALUES (@isbn, @title, @author, @category, 0)";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@isbn", txtISBN.Text);
                    cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@author", txtAuthor.Text);
                    cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Book added successfully!");
            LoadBooks();
            ClearInputs();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a book to update.");
                return;
            }

            int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["BookId"].Value);

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "UPDATE Books SET ISBN=@isbn, Title=@title, Author=@author, Category=@category " +
                               "WHERE BookId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@isbn", txtISBN.Text);
                    cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@author", txtAuthor.Text);
                    cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Book updated successfully!");
            LoadBooks();
            ClearInputs();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a book to delete.");
                return;
            }

            int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["BookId"].Value);

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "DELETE FROM Books WHERE BookId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Book deleted successfully!");
            LoadBooks();
            ClearInputs();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadBooks(txtTitle.Text);
        }

        private void dgvBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtISBN.Text = dgvBooks.Rows[e.RowIndex].Cells["ISBN"].Value.ToString();
                txtTitle.Text = dgvBooks.Rows[e.RowIndex].Cells["Title"].Value.ToString();
                txtAuthor.Text = dgvBooks.Rows[e.RowIndex].Cells["Author"].Value.ToString();
                txtCategory.Text = dgvBooks.Rows[e.RowIndex].Cells["Category"].Value.ToString();
            }
        }

        private void ClearInputs()
        {
            txtISBN.Clear();
            txtTitle.Clear();
            txtAuthor.Clear();
            txtCategory.Clear();
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {

        }

        private void dgvBooks_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtISBN.Text = dgvBooks.Rows[e.RowIndex].Cells["ISBN"].Value.ToString();
                txtTitle.Text = dgvBooks.Rows[e.RowIndex].Cells["Title"].Value.ToString();
                txtAuthor.Text = dgvBooks.Rows[e.RowIndex].Cells["Author"].Value.ToString();
                txtCategory.Text = dgvBooks.Rows[e.RowIndex].Cells["Category"].Value.ToString();
            }
        }
    }
}
