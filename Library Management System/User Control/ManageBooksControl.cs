using Library_Management_System.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Guna.UI2.WinForms.Suite.Descriptions;

namespace LibraryManagementSystem
{
    public partial class ManageBooksControl : UserControl
    {
        private readonly ErrorProvider errorProvider1 = new ErrorProvider();
        private Label lblNoBooksFound;
        private FlowLayoutPanel flowBooks;

        private int targetWidth = 150; // final width when fully shown
        private Timer slideTimer;



        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public ManageBooksControl()
        {
            InitializeComponent();
            //dgvBooks.CellPainting += DgvBooks_CustomButtonColor;
            btnAdd.ApplyRoundedCorners(10, Color.Black, 2); // radius 12

            // Initially collapsed
            panel1.Width = 0;

            // Setup Timer
            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;


            flowBooks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(flowBooks);

            // DataGridView setup
            dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            dgvBooks.EnableHeadersVisualStyles = false;
            dgvBooks.RowTemplate.Height = 40;
            dgvBooks.ReadOnly = true;
            dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.AllowUserToAddRows = false;
            dgvBooks.AllowUserToDeleteRows = false;
            DataGridViewHelper.ApplyDefaultStyle(dgvBooks);

            dgvBooks.CellPainting += DgvBooks_CellPainting_HeaderAction;
            dgvBooks.Paint += DgvBooks_Paint_ActionHeader;
            dgvBooks.Scroll += (s, e) => dgvBooks.Invalidate();
            dgvBooks.ColumnWidthChanged += (s, e) => dgvBooks.Invalidate();
            dgvBooks.CellFormatting += DgvBooks_CellFormatting;
            dgvBooks.CellContentClick += DgvBooks_CellContentClick;
            dgvBooks.DataBindingComplete += (s, e) => dgvBooks.ClearSelection();

            // Initialize the "No books found" label
            lblNoBooksFound = new Label
            {
                Text = "No books found",
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Bold | FontStyle.Italic),
                ForeColor = Color.Gray,
                Visible = false
            };

            // Position it below the search textbox
            lblNoBooksFound.Location = new Point(txtSearch.Left, txtSearch.Bottom + 5);
            this.Controls.Add(lblNoBooksFound);

            SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 0, "Search books...");
            txtSearch.KeyDown += TxtSearch_KeyDown;
            txtSearch.TextChanged += (s, e) => PerformSearch();

            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            LoadBooksAsync("").ConfigureAwait(false);
        }

        //private void DgvBooks_CustomButtonColor(object sender, DataGridViewCellPaintingEventArgs e)
        //{
        //    if (e.RowIndex < 0) return;

        //    string col = dgvBooks.Columns[e.ColumnIndex].Name;

        //    if (col != "Edit" && col != "Delete") return;

        //    e.PaintBackground(e.CellBounds, true);

        //    Color backColor;
        //    Color textColor;
        //    Color borderColor;

        //    // 🎨 Custom colors
        //    if (col == "Edit")
        //    {
        //        backColor = Color.FromArgb(242, 229, 217);  // light blue
        //        borderColor = Color.LightBlue;
        //        textColor = Color.Black;
        //    }
        //    else // Delete
        //    {
        //        backColor = Color.FromArgb(242, 229, 217);  // light red
        //        borderColor = Color.IndianRed;
        //        textColor = Color.Black;
        //    }

        //    using (SolidBrush br = new SolidBrush(backColor))
        //        e.Graphics.FillRectangle(br, e.CellBounds);

        //    // center text
        //    TextRenderer.DrawText(
        //        e.Graphics,
        //        col,
        //        new Font("Segoe UI", 9, FontStyle.Bold),
        //        e.CellBounds,
        //        textColor,
        //        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
        //    );

        //    e.Handled = true;
        //}


        #region Load Books

        public async Task LoadBooksAsync(string search = "")
        {
            try
            {
                using (SQLiteConnection con = Db.GetConnection())
                {
                    await con.OpenAsync();

                    string query = @"
                        SELECT BookId, ISBN, Title, Author, Category, Quantity, AvailableCopies,
                            CASE 
                                WHEN EXISTS (SELECT 1 FROM Reservations r WHERE r.BookId = b.BookId AND r.Status = 'Active') THEN 'Reserved'
                                WHEN AvailableCopies = 0 THEN 'Not Available'
                                ELSE 'Available'
                            END AS Status
                        FROM Books b";

                    if (!string.IsNullOrWhiteSpace(search))
                        query += " WHERE Title LIKE @search OR Author LIKE @search OR Category LIKE @search OR ISBN LIKE @search";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        if (!string.IsNullOrWhiteSpace(search))
                            cmd.Parameters.AddWithValue("@search", "%" + search + "%");

                        using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            dgvBooks.DataSource = dt;

                            dgvBooks.Columns["AvailableCopies"].HeaderText = "Available Copies";

                            // Hide "No books found" label if rows exist
                            dgvBooks.Paint += (s, e) => DrawNoBooksMessage(dgvBooks, e);



                            if (dgvBooks.Columns.Contains("BookId"))
                                dgvBooks.Columns["BookId"].Visible = false;

                            SetupActionButtons();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading books: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DrawNoBooksMessage(DataGridView dgv, PaintEventArgs e)
        {
            if (dgv.Rows.Count == 0)
            {
                string message = "No books found";
                Image icon = Library_Management_System.Properties.Resources.NoBooksIcon;

                // Scale icon to 20% of DataGridView width, but not larger than 150x150
                int iconWidth = Math.Min((int)(dgv.ClientSize.Width * 0.2), 150);
                int iconHeight = (int)(iconWidth * ((float)icon.Height / icon.Width)); // maintain aspect ratio

                // Center coordinates
                int iconX = (dgv.ClientSize.Width - iconWidth) / 2;
                int iconY = (dgv.ClientSize.Height - iconHeight) / 2 - 20; // slightly above center for label

                e.Graphics.DrawImage(icon, new Rectangle(iconX, iconY, iconWidth, iconHeight));

                // Adjust font size based on DataGridView width
                float fontSize = Math.Min(dgv.ClientSize.Width / 25f, 24f); // max 24pt
                using (Font font = new Font("Segoe UI", fontSize, FontStyle.Bold | FontStyle.Italic))
                using (SolidBrush brush = new SolidBrush(Color.Gray))
                {
                    SizeF textSize = e.Graphics.MeasureString(message, font);
                    float textX = (dgv.ClientSize.Width - textSize.Width) / 2;
                    float textY = iconY + iconHeight + 10; // 10px spacing below icon
                    e.Graphics.DrawString(message, font, brush, textX, textY);
                }
            }
        }


        private void SetupActionButtons()
        {
            if (dgvBooks.Columns.Contains("Edit")) dgvBooks.Columns.Remove("Edit");
            if (dgvBooks.Columns.Contains("Delete")) dgvBooks.Columns.Remove("Delete");
            if (dgvBooks.Columns.Contains("View")) dgvBooks.Columns.Remove("View");

            // ➤ View button
            DataGridViewButtonColumn viewCol = new DataGridViewButtonColumn
            {
                Name = "View",
                HeaderText = "",
                Text = "View",
                UseColumnTextForButtonValue = true
            };
            dgvBooks.Columns.Add(viewCol);

            DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
            {
                Name = "Edit",
                HeaderText = "",
                Text = "Edit",
                UseColumnTextForButtonValue = true,

            };
            dgvBooks.Columns.Add(editCol);

            DataGridViewButtonColumn deleteCol = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "",
                Text = "Delete",
                UseColumnTextForButtonValue = true,

            };
            dgvBooks.Columns.Add(deleteCol);
            viewCol.Width = 70;
            editCol.Width = 70;
            deleteCol.Width = 70;
            dgvBooks.ClearSelection();
        }

        #endregion

        #region DataGridView Formatting

        private void DgvBooks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return; // skip header

            string colName = dgvBooks.Columns[e.ColumnIndex].Name;


            // Color View button
            if (colName == "View")
            {
                e.CellStyle.BackColor = Color.LightGreen;
                e.CellStyle.ForeColor = Color.LightGreen;
                e.CellStyle.SelectionBackColor = Color.Green;
                e.CellStyle.Font = new Font(dgvBooks.Font, FontStyle.Regular);
                e.FormattingApplied = true;
            }

            // Color Edit button
            if (colName == "Edit")
            {
                e.CellStyle.BackColor = Color.LightBlue;      // button background
                e.CellStyle.ForeColor = Color.LightBlue;          // text color
                e.CellStyle.SelectionBackColor = Color.DodgerBlue; // hover selection
                e.CellStyle.Font = new Font(dgvBooks.Font, FontStyle.Regular); // bold text
                e.FormattingApplied = true;
            }
            // Color Delete button
            else if (colName == "Delete")
            {
                e.CellStyle.BackColor = Color.IndianRed;
                e.CellStyle.ForeColor = Color.IndianRed;
                e.CellStyle.SelectionBackColor = Color.Red;
                e.CellStyle.Font = new Font(dgvBooks.Font, FontStyle.Regular);
                e.FormattingApplied = true;
            }


            if (dgvBooks.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                Color color;
                switch (status)
                {
                    case "Available":
                        color = Color.Green;
                        break;
                    case "Not Available":
                        color = Color.Red;
                        break;
                    case "Reserved":
                        color = Color.Orange;
                        break;
                    default:
                        color = Color.Black;
                        break;
                }


                e.CellStyle.ForeColor = color;
                e.FormattingApplied = true;
            }
        }

        private void DgvBooks_CellPainting_HeaderAction(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0 && e.ColumnIndex < dgvBooks.Columns.Count)
            {
                string colName = dgvBooks.Columns[e.ColumnIndex].Name;
                if (colName == "Edit" || colName == "Delete")
                    e.Handled = true;


            }
        }

        private void DgvBooks_Paint_ActionHeader(object sender, PaintEventArgs e)
        {
            if (!dgvBooks.Columns.Contains("View") ||
                !dgvBooks.Columns.Contains("Edit") ||
                !dgvBooks.Columns.Contains("Delete"))
                return;

            // Get all three action column rectangles
            Rectangle viewCol = dgvBooks.GetCellDisplayRectangle(dgvBooks.Columns["View"].Index, -1, true);
            Rectangle editCol = dgvBooks.GetCellDisplayRectangle(dgvBooks.Columns["Edit"].Index, -1, true);
            Rectangle deleteCol = dgvBooks.GetCellDisplayRectangle(dgvBooks.Columns["Delete"].Index, -1, true);

            // Entire Action header rectangle
            Rectangle actionHeader = new Rectangle(
                viewCol.X,
                viewCol.Y,
                (deleteCol.X + deleteCol.Width) - viewCol.X,
                viewCol.Height
            );

            // Draw background + border
            e.Graphics.FillRectangle(Brushes.LightGray, actionHeader);
            e.Graphics.DrawRectangle(Pens.Gray, actionHeader);

            // Draw the "Action" label centered
            TextRenderer.DrawText(
                e.Graphics,
                "Action",
                new Font("Segoe UI", 10, FontStyle.Regular),
                actionHeader,
                Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }


        #endregion

        #region Search

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            LoadBooksAsync(txtSearch.Text.Trim()).ConfigureAwait(false);
        }

        #endregion

        #region Add/Edit/Delete

        private async void btnAdd_Click(object sender, EventArgs e)
        {

            using (Add_Book addBookForm = new Add_Book())
            {
                if (addBookForm.ShowDialog() == DialogResult.OK)
                {
                    await InsertOrUpdateBook(
                        addBookForm.ISBN,
                        addBookForm.Title,
                        addBookForm.Author,
                        addBookForm.Category,
                        addBookForm.Quantity,
                        "Book added successfully!"
                    );

                    await LoadBooksAsync();
                }
            }
        }




        private async Task InsertOrUpdateBook(string isbn, string title, string author, string category, int quantity, string successMessage)
        {
            using (SQLiteConnection con = Db.GetConnection())
            {
                await con.OpenAsync();

                string checkQuery = "SELECT BookId, Quantity, AvailableCopies FROM Books WHERE ISBN=@isbn";
                using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@isbn", isbn);
                    using (SQLiteDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int bookId = Convert.ToInt32(reader["BookId"]);
                            int newQty = Convert.ToInt32(reader["Quantity"]) + quantity;
                            int newAvailable = Convert.ToInt32(reader["AvailableCopies"]) + quantity;
                            reader.Close();

                            string updateQuery = "UPDATE Books SET Quantity=@qty, AvailableCopies=@available WHERE BookId=@id";
                            using (SQLiteCommand updateCmd = new SQLiteCommand(updateQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@qty", newQty);
                                updateCmd.Parameters.AddWithValue("@available", newAvailable);
                                updateCmd.Parameters.AddWithValue("@id", bookId);
                                updateCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Book quantity updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            reader.Close();

                            string insertQuery = "INSERT INTO Books (ISBN, Title, Author, Category, Quantity, AvailableCopies, CreatedAt) " +
                                                 "VALUES (@isbn,@title,@author,@category,@qty,@available,@createdAt)";
                            using (SQLiteCommand insertCmd = new SQLiteCommand(insertQuery, con))
                            {
                                insertCmd.Parameters.AddWithValue("@isbn", isbn);
                                insertCmd.Parameters.AddWithValue("@title", title);
                                insertCmd.Parameters.AddWithValue("@author", author);
                                insertCmd.Parameters.AddWithValue("@category", category);
                                insertCmd.Parameters.AddWithValue("@qty", quantity);
                                insertCmd.Parameters.AddWithValue("@available", quantity);
                                insertCmd.Parameters.AddWithValue("@createdAt", DateTime.Now);
                                insertCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
        }

        private async void DgvBooks_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int bookId = Convert.ToInt32(dgvBooks.Rows[e.RowIndex].Cells["BookId"].Value);
            string colName = dgvBooks.Columns[e.ColumnIndex].Name;

            if (colName == "Edit")
            {
                OpenEditForm(bookId);
            }
            else if (colName == "Delete")
            {
                DeleteBook(bookId);
            }
            else
            {
                // Get the list of borrowers for the selected book
                await ShowBookBorrowers(bookId);
            }
        }

        private async Task ShowBookBorrowers(int bookId)
        {
            try
            {
                using (SQLiteConnection con = Db.GetConnection())
                {
                    await con.OpenAsync();

                    string query = @"
                SELECT 
                    u.FirstName || ' ' || u.LastName AS FullName,
                    b.BorrowDate,
                    b.DueDate
                FROM Borrowings b
                JOIN Members u ON b.MemberId = u.MemberId
                WHERE b.BookId = @bookId AND b.Status = 'Borrowed'";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@bookId", bookId);

                        using (SQLiteDataReader reader = (SQLiteDataReader)await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                StringBuilder borrowersList = new StringBuilder();

                                while (await reader.ReadAsync())
                                {
                                    string fullName = reader["FullName"].ToString();
                                    DateTime borrowedDate = Convert.ToDateTime(reader["BorrowDate"]);
                                    DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);

                                    borrowersList.AppendLine(
                                        $"{fullName} - Borrowed on {borrowedDate.ToShortDateString()} - Due on {dueDate.ToShortDateString()}"
                                    );
                                }

                                MessageBox.Show(borrowersList.ToString(), "Borrowers", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No active borrowings for this book.", "No Borrowers", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving borrowers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void OpenEditForm(int bookId)
        {
            using (FrmEditBook frm = new FrmEditBook(bookId, this))
            {
                frm.ShowDialog();
            }
            LoadBooksAsync().ConfigureAwait(false);
        }

        private void DeleteBook(int bookId)
        {
            if (MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                return;

            using (SQLiteConnection con = Db.GetConnection())
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM Books WHERE BookId=@id", con))
                {
                    cmd.Parameters.AddWithValue("@id", bookId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Book deleted successfully!");
            LoadBooksAsync().ConfigureAwait(false);
        }

        #endregion

        private void ManageBooksControl_Load(object sender, EventArgs e)
        {

        }

        private void dgvBooks_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            if (panel1.Width < targetWidth)
            {
                panel1.Width += 8; // adjust speed
                panel1.Invalidate(); // redraw rounded corners
            }
            else
            {
                slideTimer.Stop(); // stop when fully expanded
            }
        }

        // Call this from MainForm
        public void RollOutPanel()
        {
            // Reset panel to collapsed before rolling out
            if (panel1.Width != 0)
            {
                panel1.Width = 0;
                panel1.Invalidate();
            }

            slideTimer.Start();
        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            int radius = 20;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();

            // Only round top-right and bottom-right corners
            path.AddLine(0, 0, panel1.Width - radius, 0);                    // top edge
            path.AddArc(panel1.Width - radius, 0, radius, radius, 270, 90); // top-right
            path.AddLine(panel1.Width, radius, panel1.Width, panel1.Height - radius); // right edge
            path.AddArc(panel1.Width - radius, panel1.Height - radius, radius, radius, 0, 90); // bottom-right
            path.AddLine(panel1.Width - radius, panel1.Height, 0, panel1.Height);             // bottom edge
            path.AddLine(0, panel1.Height, 0, 0);                                           // left edge
            path.CloseFigure();

            panel1.Region = new Region(path);
        }
    }
}