using System;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;

namespace Library_Management_System.Forms
{
    public partial class FrmEditBook : Form
    {
        private ErrorProvider errorProvider1 = new ErrorProvider();
        private int _bookId;
        private ManageBooksControl _parentControl;

        public FrmEditBook(int bookId, ManageBooksControl parentControl)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            _bookId = bookId;
            _parentControl = parentControl;
            LoadBookDetails();
        }

        private void LoadBookDetails()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT * FROM Books WHERE BookId=@id";
                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@id", _bookId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtISBN.Text = reader["ISBN"].ToString();
                            txtTitle.Text = reader["Title"].ToString();
                            txtAuthor.Text = reader["Author"].ToString();
                            txtCategory.Text = reader["Category"].ToString();
                            txtQuantity.Text = reader["Quantity"].ToString();
                        }
                    }
                }
            }


            // Disable textboxes except Quantity
            txtISBN.ReadOnly = true;
            txtTitle.ReadOnly = true;
            txtAuthor.ReadOnly = true;
            txtCategory.ReadOnly = true;

            // Optional: make disabled boxes visually distinct
            txtISBN.BackColor = SystemColors.Control;
            txtTitle.BackColor = SystemColors.Control;
            txtAuthor.BackColor = SystemColors.Control;
            txtCategory.BackColor = SystemColors.Control;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtISBN.Text) ||
                string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                string.IsNullOrWhiteSpace(txtCategory.Text) ||
                string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 1)
            {
                MessageBox.Show("Quantity must be a valid positive number.");
                return false;
            }

            if (txtISBN.Text.Length != 13)
            {
                MessageBox.Show("ISBN must be exactly 13 digits.");
                return false;
            }

            return true;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            int newQty = int.Parse(txtQuantity.Text);

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Get current Quantity & AvailableCopies
                int currentQty = 0, currentAvailable = 0;
                string getQuery = "SELECT Quantity, AvailableCopies FROM Books WHERE BookId=@id";
                using (var getCmd = new SQLiteCommand(getQuery, con))
                {
                    getCmd.Parameters.AddWithValue("@id", _bookId);
                    using (var reader = getCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentQty = Convert.ToInt32(reader["Quantity"]);
                            currentAvailable = Convert.ToInt32(reader["AvailableCopies"]);
                        }
                    }
                }

                int diff = newQty - currentQty;
                int newAvailable = currentAvailable + diff;
                if (newAvailable < 0) newAvailable = 0;

                string updateQuery = "UPDATE Books SET ISBN=@isbn, Title=@title, Author=@author, Category=@category, Quantity=@qty, AvailableCopies=@available WHERE BookId=@id";
                using (var cmd = new SQLiteCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@isbn", txtISBN.Text);
                    cmd.Parameters.AddWithValue("@title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@author", txtAuthor.Text);
                    cmd.Parameters.AddWithValue("@category", txtCategory.Text);
                    cmd.Parameters.AddWithValue("@qty", newQty);
                    cmd.Parameters.AddWithValue("@available", newAvailable);
                    cmd.Parameters.AddWithValue("@id", _bookId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Book updated successfully!");
            _parentControl.LoadBooksAsync();
            this.Close();
        }

        // --- TextBox Validations ---
        private void txtISBN_TextChanged(object sender, EventArgs e)
        {
            string digitsOnly = new string(txtISBN.Text.Where(char.IsDigit).ToArray());
            txtISBN.Text = digitsOnly;
            txtISBN.SelectionStart = txtISBN.Text.Length;
        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            string valid = new string(txtTitle.Text.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == '-' || c == ',' || c == '\'').ToArray());
            txtTitle.Text = valid;
            txtTitle.SelectionStart = txtTitle.Text.Length;
        }

        private void txtAuthor_TextChanged(object sender, EventArgs e)
        {
            string valid = new string(txtAuthor.Text.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '.').ToArray());
            txtAuthor.Text = valid;
            txtAuthor.SelectionStart = txtAuthor.Text.Length;
        }

        private void txtCategory_TextChanged(object sender, EventArgs e)
        {
            string valid = new string(txtCategory.Text.Where(c => char.IsLetter(c) || c == ' ' || c == ',' || c == '-').ToArray());
            txtCategory.Text = valid;
            txtCategory.SelectionStart = txtCategory.Text.Length;
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            string digitsOnly = new string(txtQuantity.Text.Where(char.IsDigit).ToArray());
            txtQuantity.Text = digitsOnly;
            txtQuantity.SelectionStart = txtQuantity.Text.Length;
        }

        private void FrmEditBook_Load(object sender, EventArgs e)
        {

        }
    }
}
