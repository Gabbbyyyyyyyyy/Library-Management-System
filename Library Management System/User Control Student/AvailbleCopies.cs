using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibraryManagementSystem.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Library_Management_System.User_Control_Student
{
    public partial class AvailbleCopies : UserControl
    {
        private string _studentNo;
        private FlowLayoutPanel flowBooks;
        private readonly string coverFolder = Path.Combine(Application.StartupPath, "BookCovers");
        private readonly string cacheFile = Path.Combine(Application.StartupPath, "BookCache.json");
        private readonly int RefreshDays = 1; // auto-refresh interval in days
        private static bool _booksLoaded = false;
        private static List<Panel> _cachedBookCards = new List<Panel>();
        private System.Windows.Forms.Timer _searchTimer;

        private class BookCache
        {
            public string ISBN { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string Status { get; set; }
        }

       
        public string StudentNo
        {
            get => _studentNo;
            set
            {
                _studentNo = value;
                LoadAvailableCopies();
            }
        }

        public AvailbleCopies()
        {
            InitializeComponent();

            // Load categories and sort options
            LoadCategoryComboBox();
            LoadSortComboBox();

            // ComboBox selection changed events
            comboBox1.SelectedIndexChanged += (s, e) => RunSearchNow();
            comboBox2.SelectedIndexChanged += (s, e) => RunSearchNow();

            SendMessage(textBox1.Handle, EM_SETCUEBANNER, 0, "Search books...");
            textBox1.KeyDown += txtSearch_KeyDown;
            textBox1.TextChanged += (s, e) => PerformSearch();


            Dock = DockStyle.Fill;

            if (!Directory.Exists(coverFolder))
                Directory.CreateDirectory(coverFolder);

            // Create container panel to control spacing
            Panel container = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 150, 0, 0), // 👈 adds 100px top space
                BackColor = Color.Gainsboro
            };

            flowBooks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(20),
                BackColor = Color.Gainsboro
            };

            container.Controls.Add(flowBooks);
            Controls.Add(container);
        }

        // 🧠 Real-time search with debounce
        private void PerformSearch()
        {
            if (_searchTimer == null)
            {
                _searchTimer = new System.Windows.Forms.Timer
                {
                    Interval = 300 // ms delay
                };
                _searchTimer.Tick += (s, e) =>
                {
                    _searchTimer.Stop();
                    RunSearchNow();
                };
            }

            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void RunSearchNow()
        {
            string searchText = textBox1.Text.Trim();
            string selectedCategory = comboBox1.SelectedItem.ToString();
            string selectedSort = comboBox2.SelectedItem.ToString();

            flowBooks.Controls.Clear();
            flowBooks.SuspendLayout();

            bool foundAny = false;

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Build query dynamically
                string query = @"
            SELECT ISBN, Title, Author, AvailableCopies, Category,
                   CASE WHEN AvailableCopies > 0 THEN 'Available' ELSE 'Not Available' END AS Status
            FROM Books
            WHERE (Title LIKE @search COLLATE NOCASE
               OR Author LIKE @search COLLATE NOCASE
               OR ISBN LIKE @search COLLATE NOCASE)";

                // Apply category filter if not "All"
                if (selectedCategory != "All")
                    query += " AND Category = @category";

                // Apply sorting
                switch (selectedSort)
                {
                    case "Title (A-Z)":
                        query += " ORDER BY Title ASC";
                        break;
                    case "Title (Z-A)":
                        query += " ORDER BY Title DESC";
                        break;
                    case "Default":
                        query += " ORDER BY ROWID ASC"; // Default order by insertion
                        break;
                    case "Most Borrowed":
                        query = @"
        SELECT b.ISBN, b.Title, b.Author, b.AvailableCopies, b.Category,
               CASE WHEN b.AvailableCopies > 0 THEN 'Available' ELSE 'Not Available' END AS Status,
               COUNT(br.BookId) AS BorrowCount
        FROM Books b
        LEFT JOIN Borrowings br ON br.BookId = b.BookId";

                        if (comboBox1.SelectedItem?.ToString() != "All")
                            query += " WHERE b.Category = @category";

                        query += @"
        GROUP BY b.BookId
        ORDER BY BorrowCount DESC, b.Title ASC"; // secondary sort by title
                        break;
                }

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    if (comboBox1.SelectedItem?.ToString() != "All")
                        cmd.Parameters.AddWithValue("@category", comboBox1.SelectedItem.ToString());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string author = reader["Author"].ToString();
                            string isbn = reader["ISBN"].ToString();
                            string status = reader["Status"].ToString();
                            string category = reader["Category"].ToString();

                            Image cover = LoadLocalCover(isbn) ?? Properties.Resources.Bc;
                            flowBooks.Controls.Add(AddBookCard(title, author, status, cover, isbn, category));
                        }
                    }
                }
            }

            if (!foundAny)
            {
                Panel noResultPanel = new Panel
                {
                    Size = new Size(flowBooks.ClientSize.Width - 40, 250),
                    Margin = new Padding(10),
                    BackColor = Color.Transparent
                };

                PictureBox pic = new PictureBox
                {
                    Image = Properties.Resources.NoBooksIcon,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(100, 100),
                    Location = new Point((noResultPanel.Width - 100) / 2, 20)
                };
                noResultPanel.Controls.Add(pic);

                Label msg = new Label
                {
                    Text = "No books showing",
                    Font = new Font("Segoe UI", 28, FontStyle.Bold | FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // Temporarily add it to panel to calculate size
                noResultPanel.Controls.Add(msg);
                msg.Location = new Point(
                    pic.Left + (pic.Width - msg.Width) / 2, // center under the PictureBox
                    pic.Bottom + 20                          // 20px spacing below image
                );

                flowBooks.Controls.Add(noResultPanel);
            }


            flowBooks.ResumeLayout();
        }


        // Handles displaying books from the reader
        private void DisplayBooks(SQLiteDataReader reader)
        {
            bool found = false;

            while (reader.Read())
            {
                found = true;
                string title = reader["Title"].ToString();
                string author = reader["Author"].ToString();
                string isbn = reader["ISBN"].ToString();
                string status = reader["Status"].ToString();
                string category = reader["Category"].ToString();

                Image cover = LoadLocalCover(isbn) ?? Properties.Resources.Bc;
                flowBooks.Controls.Add(AddBookCard(title, author, status, cover, isbn, category));
            }

            if (!found)
                ShowNoBooksMessage();
        }

        // Shows a centered "No books found" message
        private void ShowNoBooksMessage()
        {
            flowBooks.Controls.Add(new Label
            {
                Text = "No books found.",
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
        }



        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;


        private async void LoadAvailableCopies()
        {
            try
            {
                // 🧩 Always check DB for up-to-date availability
                flowBooks.Controls.Clear();
                _cachedBookCards.Clear();

                List<BookCache> bookList = new List<BookCache>();

                using (var con = Db.GetConnection())
                {
                    con.Open();
                    EnsureLastCoverUpdateColumn(con);

                    string query = @"
            SELECT 
                ISBN, Title, Author, AvailableCopies, Category,
                IFNULL(CoverUrl, '') AS CoverUrl,
                IFNULL(LastCoverUpdate, '') AS LastCoverUpdate,
                CASE WHEN AvailableCopies > 0 THEN 'Available'
                     ELSE 'Not Available' END AS Status
            FROM Books;";

                    using (var cmd = new SQLiteCommand(query, con))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string author = reader["Author"].ToString();
                            string isbn = reader["ISBN"].ToString();
                            string status = reader["Status"].ToString(); // ✅ always computed live
                            string coverUrl = reader["CoverUrl"].ToString();
                            string lastUpdateStr = reader["LastCoverUpdate"].ToString();
                            string category = reader["Category"].ToString();

                            DateTime? lastUpdate = null;
                            if (DateTime.TryParse(lastUpdateStr, out DateTime parsed))
                                lastUpdate = parsed;

                            Image cover = LoadLocalCover(isbn);
                            var card = AddBookCard(title, author, status, cover ?? Properties.Resources.Bc, isbn, category);
                            flowBooks.Controls.Add(card);
                            _cachedBookCards.Add(card);

                            // cache minimal info (exclude status)
                            bookList.Add(new BookCache
                            {
                                ISBN = isbn,
                                Title = title,
                                Author = author
                            });

                            // background cover refresh
                            _ = Task.Run(async () =>
                            {
                                bool shouldUpdate = cover == null ||
                                                    !lastUpdate.HasValue ||
                                                    (DateTime.Now - lastUpdate.Value).TotalDays >= RefreshDays;

                                if (shouldUpdate)
                                {
                                    using (var bgCon = Db.GetConnection())
                                    {
                                        bgCon.Open();
                                        Image newCover = await TryUpdateCoverAsync(isbn, coverUrl, bgCon);
                                        if (newCover != null)
                                        {
                                            if (card.InvokeRequired)
                                            {
                                                card.Invoke(new Action(() =>
                                                {
                                                    var pic = card.Controls[0] as PictureBox;
                                                    if (pic != null) pic.Image = newCover;
                                                }));
                                            }

                                            using (var cmdDate = new SQLiteCommand(
                                                "UPDATE Books SET LastCoverUpdate = @date WHERE ISBN = @isbn", bgCon))
                                            {
                                                cmdDate.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                                cmdDate.Parameters.AddWithValue("@isbn", isbn);
                                                cmdDate.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    }
                }

                // ✅ Save updated book list (without status)
                File.WriteAllText(cacheFile, JsonConvert.SerializeObject(bookList));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading available books: " + ex.Message);
            }
        }


        private void EnsureLastCoverUpdateColumn(SQLiteConnection con)
        {
            using (var cmdCheck = new SQLiteCommand("PRAGMA table_info(Books);", con))
            using (var reader = cmdCheck.ExecuteReader())
            {
                bool hasColumn = false;
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "LastCoverUpdate")
                    {
                        hasColumn = true;
                        break;
                    }
                }

                if (!hasColumn)
                {
                    using (var cmdAdd = new SQLiteCommand("ALTER TABLE Books ADD COLUMN LastCoverUpdate TEXT;", con))
                        cmdAdd.ExecuteNonQuery();
                }
            }
        }

        private async Task BackgroundRefreshAsync()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = "SELECT ISBN, CoverUrl, LastCoverUpdate FROM Books";
                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string isbn = reader["ISBN"].ToString();
                        string coverUrl = reader["CoverUrl"].ToString();
                        string lastUpdateStr = reader["LastCoverUpdate"].ToString();

                        DateTime? lastUpdate = null;
                        DateTime parsed;
                        if (DateTime.TryParse(lastUpdateStr, out parsed))
                            lastUpdate = parsed;


                        bool shouldUpdate = !lastUpdate.HasValue ||
                                            (DateTime.Now - lastUpdate.Value).TotalDays >= RefreshDays;

                        if (shouldUpdate)
                            await TryUpdateCoverAsync(isbn, coverUrl, con);
                    }
                }
            }
        }

        private Panel AddBookCard(string title, string author, string status, Image cover, string isbn = null, string category = "")
        {
            Panel card = new Panel
            {
                Width = 160,
                Height = 280,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            PictureBox pic = new PictureBox
            {
                Width = 150,
                Height = 200,
                Image = Properties.Resources.admin, // smoother scroll
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(5, 5),
                Cursor = Cursors.Hand // 👈 makes it look clickable
            };

            Task.Run(() =>
            {
                if (cover != null)
                    SafeInvoke(card, () => pic.Image = cover);
            });

            // 👇 Add click event for the book image
            pic.Click += (s, e) =>
            {
                ShowBookDetails(isbn, title, author, status, cover, category);
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = false,
                Width = 150,
                Height = 35,
                Location = new Point(5, 220),
                TextAlign = ContentAlignment.MiddleCenter
            };

            //Label lblAuthor = new Label
            //{
            //    Text = author,
            //    Font = new Font("Segoe UI", 8),
            //    ForeColor = Color.Gray,
            //    AutoSize = false,
            //    Width = 150,
            //    Height = 20,
            //    Location = new Point(5, 240),
            //    TextAlign = ContentAlignment.MiddleCenter
            //};

            Label lblStatus = new Label
            {
                Text = status,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = status == "Available" ? Color.Green : Color.Red,
                AutoSize = false,
                Width = 150,
                Height = 20,
                Location = new Point(5, 255),
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(pic);
            card.Controls.Add(lblTitle);
            //card.Controls.Add(lblAuthor);
            card.Controls.Add(lblStatus);
            return card;
        }


        private void ShowBookDetails(string isbn, string title, string author, string status, Image cover, string category)
        {
            // Example: simple pop-up (you can replace this with a custom form later)
            Form detailsForm = new Form
            {
                Text = "Book Details",
                Size = new Size(400, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White
            };

            PictureBox pic = new PictureBox
            {
                Image = cover,
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = 150,
                Height = 220,
                Location = new Point((detailsForm.ClientSize.Width - 150) / 2, 20)
            };
            detailsForm.Controls.Add(pic);

            Label lblTitle = new Label
            {
                Text = $"Title: {title}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = false,
                Width = 350,
                Location = new Point(20, 260)
            };
            detailsForm.Controls.Add(lblTitle);

            Label lblAuthor = new Label
            {
                Text = $"Author: {author}",
                Font = new Font("Segoe UI", 9),
                AutoSize = false,
                Width = 350,
                Location = new Point(20, 290)
            };
            detailsForm.Controls.Add(lblAuthor);

            Label lblStatus = new Label
            {
                Text = $"Status: {status}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = status == "Available" ? Color.Green : Color.Red,
                AutoSize = false,
                Width = 350,
                Location = new Point(20, 320)
            };
            detailsForm.Controls.Add(lblStatus);

            Label lblCategory = new Label
            {
                Text = $"Category: {category}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false,
                Width = 350,
                Location = new Point(20, 380)
            };
            detailsForm.Controls.Add(lblCategory);

            Label lblISBN = new Label
            {
                Text = $"ISBN: {isbn}",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false,
                Width = 350,
                Location = new Point(20, 350)
            };
            detailsForm.Controls.Add(lblISBN);

            Button btnClose = new Button
            {
                Text = "Close",
                Width = 100,
                Height = 35,
                Location = new Point((detailsForm.ClientSize.Width - 100) / 2, 410),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => detailsForm.Close();
            detailsForm.Controls.Add(btnClose);

            detailsForm.ShowDialog();
        }


        private Image LoadLocalCover(string isbn)
        {
            string localPath = Path.Combine(coverFolder, isbn + ".jpg");
            if (File.Exists(localPath))
            {
                using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
                    return Image.FromStream(fs);
            }
            return null;
        }

        private async Task<Image> TryUpdateCoverAsync(string isbn, string dbCoverUrl, SQLiteConnection con)
        {
            try
            {
                string localPath = Path.Combine(coverFolder, isbn + ".jpg");
                string coverUrl = dbCoverUrl;

                if (string.IsNullOrEmpty(coverUrl))
                {
                    coverUrl = await FetchCoverUrlAsync(isbn);
                    if (!string.IsNullOrEmpty(coverUrl))
                    {
                        string updateQuery = "UPDATE Books SET CoverUrl = @url WHERE ISBN = @isbn";
                        using (var cmd = new SQLiteCommand(updateQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@url", coverUrl);
                            cmd.Parameters.AddWithValue("@isbn", isbn);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(coverUrl))
                {
                    using (HttpClient client = new HttpClient())
                    using (var stream = await client.GetStreamAsync(coverUrl))
                    {
                        var img = Image.FromStream(stream);
                        img.Save(localPath);
                        return img;
                    }
                }
            }
            catch { /* silent fail */ }
            return null;
        }

        private async Task<string> FetchCoverUrlAsync(string isbn)
        {
            try
            {
                string query = Uri.EscapeDataString(isbn);
                string url = $"https://www.googleapis.com/books/v1/volumes?q={query}";

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    var json = JObject.Parse(response);
                    return json["items"]?[0]?["volumeInfo"]?["imageLinks"]?["thumbnail"]?.ToString();
                }
            }
            catch { return null; }
        }
        private void SafeInvoke(Control ctrl, Action action)
        {
            if (ctrl.IsHandleCreated)
            {
                if (ctrl.InvokeRequired)
                    ctrl.Invoke(action);
                else
                    action();
            }
            else
            {
                ctrl.HandleCreated += (s, e) =>
                {
                    if (ctrl.IsHandleCreated)
                        ctrl.BeginInvoke(action);
                };
            }
        }

        private void AvailbleCopies_Load(object sender, EventArgs e)
        {

        }

        private void LoadCategoryComboBox()
        {
            // Clear existing items first
            comboBox1.Items.Clear();

            // Add categories
            string[] categories = new string[]
            {
        "All",
        "Computers",
        "Mathematics",
        "Science",
        "History",
        "Fiction",
        "Romance"
            };

            comboBox1.Items.AddRange(categories);

            // Set default selected item
            comboBox1.SelectedIndex = 0; // "All"
        }

        private void LoadSortComboBox()
        {
            // Clear existing items first
            comboBox2.Items.Clear();

            // Add sort options
            string[] sortOptions = new string[]
            {
         "Default",
        "Title (A-Z)",
        "Title (Z-A)",
        "Most Borrowed"
            };

            comboBox2.Items.AddRange(sortOptions);

            // Set default selected item
            comboBox2.SelectedIndex = 0; // "Title (A-Z)"
        }

    }
}
