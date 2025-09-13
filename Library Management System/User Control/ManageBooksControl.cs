using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq; // install via NuGet
using LibraryManagementSystem.Data;
using Library_Management_System.Models;


namespace LibraryManagementSystem
{
    public partial class ManageBooksControl : UserControl
    {
        private ErrorProvider errorProvider1 = new ErrorProvider();

        public ManageBooksControl()
        {
            InitializeComponent();
            LoadBooks();

            dgvBooks.ReadOnly = true;
            dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.AllowUserToAddRows = false;
            dgvBooks.AllowUserToDeleteRows = false;
            DataGridViewHelper.ApplyDefaultStyle(dgvBooks);

            // Prevent auto-selection after loading data
            dgvBooks.DataBindingComplete += (s, e) => dgvBooks.ClearSelection();

            SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 0, "Search...");
            txtSearch.KeyDown += txtSearch_KeyDown;
            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;
        }

        private async Task FetchBookInfo(string searchText)
        {
            using (HttpClient client = new HttpClient())
            {
                // Search by title or ISBN
                string url = $"https://www.googleapis.com/books/v1/volumes?q={searchText}";
                var response = await client.GetStringAsync(url);

                JObject json = JObject.Parse(response);

                if (json["items"] != null)
                {
                    var volumeInfo = json["items"][0]["volumeInfo"];

                    // ISBN
                    var identifiers = volumeInfo["industryIdentifiers"];
                    if (identifiers != null)
                    {
                        foreach (var id in identifiers)
                        {
                            if (id["type"].ToString() == "ISBN_13")
                            {
                                txtISBN.Text = id["identifier"].ToString();
                                break;
                            }
                        }
                    }
                    // Title
                    txtTitle.Text = volumeInfo["title"]?.ToString() ?? "";

                    // Authors
                    txtAuthor.Text = volumeInfo["authors"] != null
                        ? string.Join(", ", volumeInfo["authors"])
                        : "";

                    // Category
                    txtCategory.Text = volumeInfo["categories"] != null
                        ? string.Join(", ", volumeInfo["categories"])
                        : "";
                }
                else
                {
                    MessageBox.Show("Book not found in API. Please enter manually.");
                }
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        // Load Books into DataGridView
        private void LoadBooks(string search = "")
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT BookId, ISBN, Title, Author, Category, Quantity, AvailableCopies, " +
                      "CASE WHEN AvailableCopies = 0 THEN 'Borrowed Out' ELSE 'Available' END AS Status " +
                      "FROM Books";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " WHERE Title LIKE @search OR Author LIKE @search OR Category LIKE @search OR ISBN LIKE @search";
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

                        // Remove default selection
                        if (dgvBooks.Rows.Count > 0)
                            dgvBooks.ClearSelection();
                    }
                }
            }
        }

        // When a row is clicked, fill only the Quantity textbox
        private void dgvBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtQuantity.Text = dgvBooks.Rows[e.RowIndex].Cells["Quantity"].Value.ToString();
            }
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return; // Make sure all fields are filled

            // ISBN validation: must be exactly 13 digits
            string isbn = txtISBN.Text.Trim();
            if (isbn.Length != 13 || !isbn.All(char.IsDigit))
            {
                MessageBox.Show("ISBN must contain exactly 13 numeric digits.", "Invalid ISBN", MessageBoxButtons.OK);
                txtISBN.Focus();
                return;
            }

            int qty = int.Parse(txtQuantity.Text);

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Check if book already exists by ISBN
                string checkQuery = "SELECT Title FROM Books WHERE ISBN=@isbn";
                using (var checkCmd = new SQLiteCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@isbn", txtISBN.Text.Trim());
                    var existingTitle = checkCmd.ExecuteScalar()?.ToString();

                    if (!string.IsNullOrEmpty(existingTitle))
                    {
                        if (existingTitle != txtTitle.Text.Trim())
                        {
                            MessageBox.Show("This ISBN already exists with a different title. ISBN must be unique!");
                            return; // Stop adding
                        }
                        else
                        {
                            MessageBox.Show("This book already exists. Use Update button to change quantity.");
                            return; // Stop adding
                        }
                    }
                }
                // Optional: Check for same Title & Author
                string checkTitleAuthorQuery = "SELECT COUNT(*) FROM Books WHERE Title=@title AND Author=@author";
                using (var checkCmd = new SQLiteCommand(checkTitleAuthorQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                    checkCmd.Parameters.AddWithValue("@author", txtAuthor.Text.Trim());
                    int duplicateCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (duplicateCount > 0)
                    {
                        DialogResult result = MessageBox.Show(
                            "A book with the same Title and Author already exists with a different ISBN. Add anyway?",
                            "Duplicate Book",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (result == DialogResult.No)
                            return; // Cancel insert
                    }
                }

                // Insert new book (safe, because ISBN is unique)
                string insertQuery = "INSERT INTO Books (ISBN, Title, Author, Category, Quantity, AvailableCopies) " +
                                     "VALUES (@isbn, @title, @author, @category, @qty, @qty)";
                using (var insertCmd = new SQLiteCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@isbn", txtISBN.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@title", txtTitle.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@author", txtAuthor.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@category", txtCategory.Text.Trim());
                    insertCmd.Parameters.AddWithValue("@qty", qty);
                    insertCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Book added successfully!");

                // **Clear the error icon here**
                errorProvider1.SetError(txtISBN, "");
            }

            LoadBooks();   // Refresh DataGridView
            ClearInputs(); // Clear input fields
        }



        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Step 1: Ensure a book row is selected
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a book to update.");
                return;
            }
            // Validate quantity input
            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out int newQty) || newQty < 1)
            {
                errorProvider1.SetError(txtQuantity, "Please enter a valid quantity."); // Red icon + tooltip
                MessageBox.Show("Please enter a valid quantity.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                errorProvider1.SetError(txtQuantity, ""); // Clear error
            }



            // Step 2: Ask for confirmation
            var confirm = MessageBox.Show(
                "Are you sure you want to update this book?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            // Step 3: Get the BookId from the selected row
            int bookId = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["BookId"].Value);

            // Step 4: Update the book in the database
            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = "UPDATE Books SET Quantity=@qty WHERE BookId=@id";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@qty", newQty);
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.ExecuteNonQuery();
                }
            }

            // Step 5: Show success + refresh grid
            MessageBox.Show("Book quantity updated successfully!");
            LoadBooks();
            txtQuantity.Clear(); // Clear textbox
        }



        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a book to delete.");
                return;
            }

            // Show confirmation dialog
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this book?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.No)
            {
                return; // User canceled
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
            LoadBooks(txtSearch.Text); // Make sure you add a txtSearch TextBox in your form
        }



        private void ClearInputs()
        {
            txtISBN.Clear();
            txtTitle.Clear();
            txtAuthor.Clear();
            txtCategory.Clear();
            txtQuantity.Clear();
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
            return true;
        }
        private async Task FetchAndSaveAllBooksFromAPI(params string[] categories)
        {
            try
            {
                using (var con = Db.GetConnection())
                {
                    con.Open();

                    // Prepare tasks for each category
                    var tasks = categories.Select(cat => FetchCategoryFromAPI(cat, con));
                    await Task.WhenAll(tasks); // Run all categories in parallel asynchronously
                }

                LoadBooks(); // Refresh DataGridView after fetching all categories
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching books from API: " + ex.Message);
            }
        }

        // Separate method to fetch a single category
        // Fetch a single category and save to SQLite
        private async Task FetchCategoryFromAPI(string category, SQLiteConnection con)
        {
            using (HttpClient client = new HttpClient())
            {
                int startIndex = 0;
                int maxResults = 40;
                bool moreResults = true;

                while (moreResults)
                {
                    string url = $"https://www.googleapis.com/books/v1/volumes?q={category}&startIndex={startIndex}&maxResults={maxResults}";
                    var response = await client.GetStringAsync(url);
                    JObject json = JObject.Parse(response);

                    if (json["items"] != null)
                    {
                        foreach (var item in json["items"])
                        {
                            var volumeInfo = item["volumeInfo"];

                            string isbn = "";
                            var identifiers = volumeInfo["industryIdentifiers"];
                            if (identifiers != null)
                            {
                                foreach (var id in identifiers)
                                {
                                    if (id["type"].ToString() == "ISBN_13")
                                    {
                                        isbn = id["identifier"].ToString();
                                        break;
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(isbn)) continue; // Skip if no ISBN

                            string title = volumeInfo["title"]?.ToString() ?? "Unknown Title";
                            string author = volumeInfo["authors"] != null ? string.Join(", ", volumeInfo["authors"]) : "Unknown Author";
                            string cat = volumeInfo["categories"] != null ? string.Join(", ", volumeInfo["categories"]) : "Uncategorized";
                            int qty = 1;

                            // Check if ISBN exists
                            string checkQuery = "SELECT BookId FROM Books WHERE ISBN=@isbn";
                            using (var checkCmd = new SQLiteCommand(checkQuery, con))
                            {
                                checkCmd.Parameters.AddWithValue("@isbn", isbn);
                                using (var reader = checkCmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        // Book already exists → DO NOTHING
                                        reader.Close();
                                    }
                                    else
                                    {
                                        reader.Close();

                                        // Insert new book
                                        string insertQuery = "INSERT INTO Books (ISBN, Title, Author, Category, Quantity, AvailableCopies) " +
                                                             "VALUES (@isbn, @title, @author, @category, @qty, @qty)";
                                        using (var insertCmd = new SQLiteCommand(insertQuery, con))
                                        {
                                            insertCmd.Parameters.AddWithValue("@isbn", isbn);
                                            insertCmd.Parameters.AddWithValue("@title", title);
                                            insertCmd.Parameters.AddWithValue("@author", author);
                                            insertCmd.Parameters.AddWithValue("@category", category);
                                            insertCmd.Parameters.AddWithValue("@qty", qty);
                                            insertCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }

                        // Prepare for next page
                        int itemsCount = ((JArray)json["items"]).Count;
                        startIndex += itemsCount;
                        if (itemsCount < maxResults) moreResults = false;
                    }
                    else
                    {
                        moreResults = false;
                    }
                }
            }
        }



        private void txtISBN_TextChanged(object sender, EventArgs e)
        {
            // Skip if textbox is empty
            if (string.IsNullOrWhiteSpace(txtISBN.Text))
            {
                errorProvider1.SetError(txtISBN, "");
                return;
            }
            // Check if the input contains any non-digit character
            if (txtISBN.Text.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("ISBN must contain only numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Remove non-digit characters
                string digitsOnly = new string(txtISBN.Text.Where(char.IsDigit).ToArray());
                txtISBN.Text = digitsOnly;
                txtISBN.SelectionStart = txtISBN.Text.Length; // Keep cursor at the end
                return;
            }

            // Limit to 13 digits
            if (txtISBN.Text.Length > 13)
            {
                MessageBox.Show("ISBN cannot be longer than 13 digits.", "Invalid ISBN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtISBN.Text = txtISBN.Text.Substring(0, 13);
                txtISBN.SelectionStart = txtISBN.Text.Length;
            }

            // Optional: show error icon if length is not 13
            if (txtISBN.Text.Length != 13)
                errorProvider1.SetError(txtISBN, "ISBN must be exactly 13 digits.");
            else
                errorProvider1.SetError(txtISBN, "");
        }



        private void txtAuthor_TextChanged(object sender, EventArgs e)
        {
            string validChars = new string(txtAuthor.Text
                .Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '.')
                .ToArray());

            if (txtAuthor.Text != validChars)
            {
                MessageBox.Show("Author name can only contain letters, numbers, spaces, hyphens, and periods.",
                                "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                txtAuthor.Text = validChars;
                txtAuthor.SelectionStart = txtAuthor.Text.Length; // Keep cursor at the end
            }
        }


        private async void BooksForm_Load(object sender, EventArgs e)
        {
            LoadBooks(); // Always load existing books from SQLite

            // Fetch API books in the background (multi-category, async)
            string[] categories = { "programming", "library", "mathematics", "science", "history", "fiction" };
            await FetchAndSaveAllBooksFromAPI(categories);
        }



        // Quantity area: the quantity must larger or equal than available copy. - add validation for this!
        //                if the user borrowed  books the quantity is decrease by 1 also the available copy
        //                update the quantity, if the user edit the quantity the available copy also increase. for example the quantity is 10 and available copy is 10 also if  there's a user that borrowrd book so the quantity remain 10 then the available copy decrease by 1. so the available copy is now 9, and the quantity remains 10. But if the admin edit the quantity and add 5 the available copy will also add 5 so the quantity is 15 and the available copy is 19. and if the student returned the book so the available copy will update it will go back to 20.
        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            string input = txtQuantity.Text.Trim();

            // Check if input is empty
            if (string.IsNullOrEmpty(input)) return;

            // Check if input contains non-digit characters
            if (input.Any(c => !char.IsDigit(c)))
            {
                MessageBox.Show("Quantity must contain only numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Remove non-digit characters
                string digitsOnly = new string(input.Where(char.IsDigit).ToArray());
                txtQuantity.Text = digitsOnly;
                txtQuantity.SelectionStart = txtQuantity.Text.Length;
                return;
            }

            // Check if input is 0
            if (int.TryParse(input, out int qty) && qty == 0)
            {
                MessageBox.Show("Quantity cannot be 0.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Text = ""; // clear invalid input
                return;
            }

            // Optional: clear any previous error
            errorProvider1.SetError(txtQuantity, "");
        }




        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"SELECT BookId, ISBN, Title, Author, Category, Quantity, AvailableCopies,
                                CASE WHEN AvailableCopies = 0 THEN 'Borrowed Out' ELSE 'Available' END AS Status
                         FROM Books
                         WHERE Title LIKE @search 
                            OR Author LIKE @search 
                            OR Category LIKE @search 
                            OR ISBN LIKE @search";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvBooks.DataSource = dt;

                        // Use the DataTable to check if any rows exist
                        if (dt.Rows.Count == 0)
                        {
                            lblSearchMessage.Text = "No books match your search.";
                        }
                        else
                        {
                            lblSearchMessage.Text = "";
                        }
                    }
                }
            }
        }
        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) // Only trigger search on Enter key
            {
                e.SuppressKeyPress = true; // Prevent ding sound
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            string searchText = txtSearch.Text.Trim();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"SELECT BookId, ISBN, Title, Author, Category, Quantity, AvailableCopies,
                        CASE WHEN AvailableCopies = 0 THEN 'Borrowed Out' ELSE 'Available' END AS Status
                        FROM Books
                        WHERE Title LIKE @search 
                           OR Author LIKE @search 
                           OR Category LIKE @search 
                           OR ISBN LIKE @search";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvBooks.DataSource = dt;

                        dgvBooks.DataSource = dt; // Always update DataGridView

                        if (dt.Rows.Count == 0)
                        {
                            //MessageBox.Show("No books found matching your search.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private async void btnFetchAPI_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtISBN.Text) && string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Please enter a Title or ISBN first.");
                return;
            }
            string searchQuery = !string.IsNullOrWhiteSpace(txtISBN.Text) ? txtISBN.Text : txtTitle.Text;
            await FetchBookInfo(searchQuery); // Fill txtTitle, txtAuthor, txtCategory, txtISBN

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Book not found in API. Please enter manually.");
                return;
            }
            // Default quantity = 1 if empty
            if (string.IsNullOrWhiteSpace(txtQuantity.Text) || !int.TryParse(txtQuantity.Text, out int qty))
            {
                qty = 1;
                txtQuantity.Text = "1";
            }
            using (var con = Db.GetConnection())
            {
                con.Open();

                string checkQuery = "SELECT BookId, Quantity, AvailableCopies FROM Books WHERE ISBN=@isbn";
                using (var checkCmd = new SQLiteCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@isbn", txtISBN.Text);
                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Update existing book quantity
                            int bookId = Convert.ToInt32(reader["BookId"]);
                            int newQty = Convert.ToInt32(reader["Quantity"]) + qty;
                            int newAvailable = Convert.ToInt32(reader["AvailableCopies"]) + qty;
                            reader.Close();

                            string updateQuery = "UPDATE Books SET Quantity=@qty, AvailableCopies=@available WHERE BookId=@id";
                            using (var updateCmd = new SQLiteCommand(updateQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@qty", newQty);
                                updateCmd.Parameters.AddWithValue("@available", newAvailable);
                                updateCmd.Parameters.AddWithValue("@id", bookId);
                                updateCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Book quantity updated successfully!");
                        }
                        else
                        {
                            reader.Close();

                            // Insert new book
                            string insertQuery = "INSERT INTO Books (ISBN, Title, Author, Category, Quantity, AvailableCopies) " +
                                                 "VALUES (@isbn, @title, @author, @category, @qty, @qty)";
                            using (var insertCmd = new SQLiteCommand(insertQuery, con))
                            {
                                insertCmd.Parameters.AddWithValue("@isbn", txtISBN.Text);
                                insertCmd.Parameters.AddWithValue("@title", txtTitle.Text);
                                insertCmd.Parameters.AddWithValue("@author", txtAuthor.Text);
                                insertCmd.Parameters.AddWithValue("@category", txtCategory.Text);
                                insertCmd.Parameters.AddWithValue("@qty", qty);
                                insertCmd.ExecuteNonQuery();
                            }
                            MessageBox.Show("Book added from API successfully!");
                        }
                    }
                }
            }

            LoadBooks();   // Refresh DataGridView
            ClearInputs(); // Clear input fields
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lblSearchMessage_Click(object sender, EventArgs e)
        {

        }

        private void ManageBooksControl_Load(object sender, EventArgs e)
        {
            // You can leave this empty or put initialization code here
        }


        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            string title = txtTitle.Text.Trim();

            // Allow letters, numbers, spaces, periods, hyphens, commas, apostrophes
            if (!System.Text.RegularExpressions.Regex.IsMatch(title, @"^[a-zA-Z0-9\s\.\-,'']*$"))
            {
                MessageBox.Show("Title can only contain letters, numbers, spaces, periods, hyphens, commas, and apostrophes.");

                // Remove invalid characters
                string validText = new string(title.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == '-' || c == ',' || c == '\'').ToArray());
                txtTitle.Text = validText;
                txtTitle.SelectionStart = txtTitle.Text.Length; // Keep cursor at the end
            }
        }



        private void txtCategory_TextChanged(object sender, EventArgs e)
        {
            string category = txtCategory.Text.Trim();

            // Allow only letters, spaces, hyphens, and commas
            if (!System.Text.RegularExpressions.Regex.IsMatch(category, @"^[a-zA-Z\s,-]*$"))
            {
                MessageBox.Show("Category can only contain letters, spaces, hyphens, and commas.");

                // Remove invalid characters
                string validText = new string(category.Where(c => char.IsLetter(c) || c == ' ' || c == '-' || c == ',').ToArray());

                // Temporarily remove the event handler to prevent infinite loop
                txtCategory.TextChanged -= txtCategory_TextChanged;
                txtCategory.Text = validText;
                txtCategory.SelectionStart = txtCategory.Text.Length; // Keep cursor at the end
                txtCategory.TextChanged += txtCategory_TextChanged;
            }
        }

        private void dgvBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}