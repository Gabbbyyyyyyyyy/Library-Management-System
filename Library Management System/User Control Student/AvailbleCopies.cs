using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
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
            string searchText = textBox1.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                flowBooks.Controls.Clear();
                flowBooks.Controls.AddRange(_cachedBookCards.ToArray());
                return;
            }

            flowBooks.Controls.Clear();

            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
                SELECT ISBN, Title, Author, AvailableCopies,
                CASE WHEN AvailableCopies > 0 THEN 'Available' ELSE 'Not Available' END AS Status
                FROM Books
                WHERE LOWER(Title) LIKE @search
                   OR LOWER(Author) LIKE @search
                   OR LOWER(Category) LIKE @search
                   OR LOWER(ISBN) LIKE @search";

                using (var cmd = new SQLiteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        bool found = false;

                        while (reader.Read())
                        {
                            found = true;
                            string title = reader["Title"].ToString();
                            string author = reader["Author"].ToString();
                            string isbn = reader["ISBN"].ToString();
                            string status = reader["Status"].ToString();

                            Image cover = LoadLocalCover(isbn) ?? Properties.Resources.Bc;
                            var card = AddBookCard(title, author, status, cover, isbn);
                            flowBooks.Controls.Add(card);
                        }

                        if (!found)
                        {
                            Label noResult = new Label
                            {
                                Text = "No books found.",
                                Dock = DockStyle.Fill,
                                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                                ForeColor = Color.Gray,
                                TextAlign = ContentAlignment.MiddleCenter
                            };
                            flowBooks.Controls.Add(noResult);
                        }
                    }
                }
            }
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
                // ✅ STEP 1 — Load from JSON cache instantly
                if (File.Exists(cacheFile))
                {
                    string json = File.ReadAllText(cacheFile);
                    var cachedBooks = JsonConvert.DeserializeObject<List<BookCache>>(json);

                    if (cachedBooks != null && cachedBooks.Count > 0)
                    {
                        flowBooks.Controls.Clear();
                        _cachedBookCards.Clear();

                        foreach (var b in cachedBooks)
                        {
                            Image cover = LoadLocalCover(b.ISBN) ?? Properties.Resources.Bc;
                            var card = AddBookCard(b.Title, b.Author, b.Status, cover, b.ISBN);
                            flowBooks.Controls.Add(card);
                            _cachedBookCards.Add(card);
                        }

                        // Background refresh
                        _ = Task.Run(() => BackgroundRefreshAsync());
                        _booksLoaded = true;
                        return;
                    }
                }

                // ✅ STEP 2 — Load from DB if no cache
                if (_booksLoaded)
                {
                    flowBooks.Controls.Clear();
                    flowBooks.Controls.AddRange(_cachedBookCards.ToArray());
                    return;
                }

                _booksLoaded = true;
                flowBooks.Controls.Clear();
                _cachedBookCards.Clear();

                List<BookCache> bookList = new List<BookCache>();

                using (var con = Db.GetConnection())
                {
                    con.Open();
                    EnsureLastCoverUpdateColumn(con);

                    string query = @"
                    SELECT 
                        ISBN, Title, Author, AvailableCopies,
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
                            string status = reader["Status"].ToString();
                            string coverUrl = reader["CoverUrl"].ToString();
                            string lastUpdateStr = reader["LastCoverUpdate"].ToString();

                            DateTime? lastUpdate = null;
                            DateTime parsed;
                            if (DateTime.TryParse(lastUpdateStr, out parsed))
                                lastUpdate = parsed;


                            Image cover = LoadLocalCover(isbn);
                            var card = AddBookCard(title, author, status, cover ?? Properties.Resources.Bc, isbn);
                            flowBooks.Controls.Add(card);
                            _cachedBookCards.Add(card);

                            bookList.Add(new BookCache
                            {
                                ISBN = isbn,
                                Title = title,
                                Author = author,
                                Status = status
                            });

                            _ = Task.Run(async () =>
                            {
                                bool shouldUpdate = cover == null ||
                                                    !lastUpdate.HasValue ||
                                                    (DateTime.Now - lastUpdate.Value).TotalDays >= RefreshDays;

                                if (shouldUpdate)
                                {
                                    Image newCover = await TryUpdateCoverAsync(isbn, coverUrl, con);
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

                                        string updateDateQuery = "UPDATE Books SET LastCoverUpdate = @date WHERE ISBN = @isbn";
                                        using (var cmdDate = new SQLiteCommand(updateDateQuery, con))
                                        {
                                            cmdDate.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                            cmdDate.Parameters.AddWithValue("@isbn", isbn);
                                            cmdDate.ExecuteNonQuery();
                                        }
                                    }
                                }
                            });
                        }
                    }
                }

                // ✅ STEP 3 — Save to cache for next run
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

        private Panel AddBookCard(string title, string author, string status, Image cover, string isbn = null)
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
                ShowBookDetails(isbn, title, author, status, cover);
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = false,
                Width = 150,
                Height = 35,
                Location = new Point(5, 200),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblAuthor = new Label
            {
                Text = author,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                AutoSize = false,
                Width = 150,
                Height = 20,
                Location = new Point(5, 235),
                TextAlign = ContentAlignment.MiddleCenter
            };

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
            card.Controls.Add(lblAuthor);
            card.Controls.Add(lblStatus);
            return card;
        }


        private void ShowBookDetails(string isbn, string title, string author, string status, Image cover)
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
                Location = new Point((detailsForm.ClientSize.Width - 100) / 2, 400),
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
    }
}
