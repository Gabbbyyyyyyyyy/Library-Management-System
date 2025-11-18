using System;
using System.Data.SQLite;
using System.Drawing;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
namespace Library_Management_System.Forms
    {
        public partial class Add_Book : Form
        {
            private ErrorProvider errorProvider1 = new ErrorProvider();
            public string ISBN => txtISBN.Text.Trim();
            public string Title => txtTitle.Text.Trim();
            public string Author => txtAuthor.Text.Trim();
            public string Category => txtCategory.Text.Trim();
            public int Quantity => int.TryParse(txtQuantity.Text.Trim(), out int qty) ? qty : 0;


            public Add_Book()
            {
                InitializeComponent();
                this.StartPosition = FormStartPosition.CenterScreen;
        }

        public async Task FetchBookInfoAsync(string searchText)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://www.googleapis.com/books/v1/volumes?q={searchText}";
                string response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);

                if (json["items"] != null)
                {
                    var volumeInfo = json["items"][0]["volumeInfo"];
                    // ISBN
                    txtISBN.Text = "";
                    var identifiers = volumeInfo["industryIdentifiers"];
                    if (identifiers != null)
                    {
                        foreach (var id in identifiers)
                        {
                            if (id["type"]?.ToString() == "ISBN_13")
                            {
                                txtISBN.Text = id["identifier"]?.ToString();
                                break;
                            }
                        }
                    }

                    txtTitle.Text = volumeInfo["title"]?.ToString() ?? "";
                    txtAuthor.Text = volumeInfo["authors"] != null ? string.Join(", ", volumeInfo["authors"]) : "";
                    txtCategory.Text = volumeInfo["categories"] != null ? string.Join(", ", volumeInfo["categories"]) : "";

                    if (!int.TryParse(txtQuantity.Text, out int qty))
                        txtQuantity.Text = "1";
                }
                else
                {
                    MessageBox.Show("Book not found in API. Please enter manually.");
                }
            }
        }

        private async void btnFetchAPI_Click(object sender, EventArgs e)
        {
            string search = !string.IsNullOrWhiteSpace(txtISBN.Text) ? txtISBN.Text : txtTitle.Text;
            if (string.IsNullOrWhiteSpace(search))
            {
                MessageBox.Show("Enter ISBN or Title first.");
                return;
            }

            await FetchBookInfoAsync(search);
        }
        private void Add_Book_Load(object sender, EventArgs e)
            {
                // Optional: Initialize ErrorProvider settings
                errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            btnGetBook.MouseEnter += btnGetBook_MouseEnter;
            btnGetBook.MouseLeave += btnGetBook_MouseLeave;
            lblHoverInfo.Visible = false; // hide initially
        }

        private void txtISBN_TextChanged(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(txtISBN.Text))
                {
                    errorProvider1.SetError(txtISBN, "");
                    return;
                }

                // Only digits
                if (txtISBN.Text.Any(c => !char.IsDigit(c)))
                {
                    MessageBox.Show("ISBN must contain only numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtISBN.Text = new string(txtISBN.Text.Where(char.IsDigit).ToArray());
                    txtISBN.SelectionStart = txtISBN.Text.Length;
                    return;
                }

                // Max length 13
                if (txtISBN.Text.Length > 13)
                {
                    MessageBox.Show("ISBN cannot be longer than 13 digits.", "Invalid ISBN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtISBN.Text = txtISBN.Text.Substring(0, 13);
                    txtISBN.SelectionStart = txtISBN.Text.Length;
                }

                // Error if not exactly 13 digits
                errorProvider1.SetError(txtISBN, txtISBN.Text.Length != 13 ? "ISBN must be exactly 13 digits." : "");
            }

            private void txtTitle_TextChanged(object sender, EventArgs e)
            {
                string title = txtTitle.Text;
                string validText = new string(title.Where(c => char.IsLetterOrDigit(c) || " .-,'".Contains(c)).ToArray());

                if (title != validText)
                {
                    MessageBox.Show("Title can only contain letters, numbers, spaces, periods, hyphens, commas, and apostrophes.");
                    txtTitle.Text = validText;
                    txtTitle.SelectionStart = txtTitle.Text.Length;
                }
            }

            private void txtAuthor_TextChanged(object sender, EventArgs e)
            {
                string author = txtAuthor.Text;
                string validText = new string(author.Where(c => char.IsLetterOrDigit(c) || " .-".Contains(c)).ToArray());

                if (author != validText)
                {
                    MessageBox.Show("Author name can only contain letters, numbers, spaces, periods, and hyphens.");
                    txtAuthor.Text = validText;
                    txtAuthor.SelectionStart = txtAuthor.Text.Length;
                }
            }

            private void txtCategory_TextChanged(object sender, EventArgs e)
            {
                string category = txtCategory.Text;
                string validText = new string(category.Where(c => char.IsLetter(c) || " ,-".Contains(c)).ToArray());

                if (category != validText)
                {
                    MessageBox.Show("Category can only contain letters, spaces, hyphens, and commas.");
                    txtCategory.Text = validText;
                    txtCategory.SelectionStart = txtCategory.Text.Length;
                }
            }

            private void txtQuantity_TextChanged(object sender, EventArgs e)
            {
                string input = txtQuantity.Text;

                if (string.IsNullOrEmpty(input)) return;

                if (input.Any(c => !char.IsDigit(c)))
                {
                    MessageBox.Show("Quantity must contain only numbers.");
                    txtQuantity.Text = new string(input.Where(char.IsDigit).ToArray());
                    txtQuantity.SelectionStart = txtQuantity.Text.Length;
                    return;
                }

                if (int.TryParse(txtQuantity.Text, out int qty) && qty == 0)
                {
                    MessageBox.Show("Quantity cannot be 0.");
                    txtQuantity.Clear();
                    return;
                }

                errorProvider1.SetError(txtQuantity, "");
            }

            private bool ValidateInputs()
            {
                if (string.IsNullOrWhiteSpace(txtISBN.Text) ||
                    string.IsNullOrWhiteSpace(txtTitle.Text) ||
                    string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                    string.IsNullOrWhiteSpace(txtCategory.Text) ||
                    string.IsNullOrWhiteSpace(txtQuantity.Text))
                {
                    MessageBox.Show("Please fill in all fields.", "Validation Error");
                    return false;
                }

                if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 1)
                {
                    MessageBox.Show("Quantity must be a valid positive number.", "Validation Error");
                    return false;
                }

                if (txtISBN.Text.Length != 13)
                {
                    MessageBox.Show("ISBN must be exactly 13 digits.", "Validation Error");
                    return false;
                }

                return true;
            }

       
         

            private void btnAdd_Click_1(object sender, EventArgs e)
            {
                if (!ValidateInputs()) return;

                this.DialogResult = DialogResult.OK; // Return OK to caller
                this.Close();
            }


        private async void btnGetBook_Click(object sender, EventArgs e)
        {

            // Position the hover label near the button
            lblHoverInfo.Location = new Point(btnGetBook.Left, btnGetBook.Top - lblHoverInfo.Height - 2);
            lblHoverInfo.Visible = true;
            string searchText = !string.IsNullOrWhiteSpace(txtISBN.Text) ? txtISBN.Text : txtTitle.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Instead of MessageBox, show the message in label6
                label6.Text = "Please enter an ISBN or Title to search.";
                label6.ForeColor = Color.Red; // optional: make it red for warning
                return;
            }

            try
            {
                // Call the async method to fetch book info
                await FetchBookInfoAsync(searchText);

                // Update label on successful fetch
                if (!string.IsNullOrWhiteSpace(txtTitle.Text))
                {
                    label6.Text = "Book fetched successfully!";
                    label6.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                label6.Text = "Error fetching book info: " + ex.Message;
                label6.ForeColor = Color.Red;
            }
        }

        private void btnGetBook_MouseEnter(object sender, EventArgs e)
        {
            lblHoverInfo.Location = new Point(btnGetBook.Left, btnGetBook.Top - lblHoverInfo.Height - 2);
            lblHoverInfo.Text = "Get Book from API"; // optional: set text
            lblHoverInfo.Visible = true;
        }

        private void btnGetBook_MouseLeave(object sender, EventArgs e)
        {
            lblHoverInfo.Visible = false;
        }


        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void lblHoverInfo_Click(object sender, EventArgs e)
        {

        }
    }
    }
