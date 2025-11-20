using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Library_Management_System.Models;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem
{
    public partial class ManageMembersControl : UserControl
    {
        private Timer refreshTimer;
        private bool isViewingMember = false;

        private int targetWidth = 150; // final width when fully shown
        private Timer slideTimer;


        public ManageMembersControl()
        {
            InitializeComponent();
            // Initially collapsed
            panel1.Width = 0;

            // Setup Timer
            slideTimer = new Timer();
            slideTimer.Interval = 10;
            slideTimer.Tick += SlideTimer_Tick;

            // Set search box style

            txtSearch.Font = new Font(txtSearch.Font.FontFamily, 10, txtSearch.Font.Style);
            dgvMembers.CellContentClick += DgvMembers_CellContentClick;
            dgvMembers.EnableHeadersVisualStyles = false;
            dgvMembers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            dgvMembers.CellPainting += dgvMembers_CellPainting_HeaderAction;
            dgvMembers.CellFormatting += dgvMembers_CellFormatting;
   
            dgvMembers.CellDoubleClick += dgvMembers_CellDoubleClick;
            dgvMembers.Paint += DgvMembers_Paint_ActionHeader;

            dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMembers.MultiSelect = false;
            dgvMembers.ClearSelection();
            dgvMembers.CurrentCell = null;



            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            DataGridViewHelper.ApplyDefaultStyle(dgvMembers);
            TextBoxHelper.ApplySearchBox(txtSearch, "Search members...", txtSearch_KeyDown);
            txtSearch.ForeColor = Color.Black;
            txtSearch.TextChanged += txtSearch_TextChanged;
            lblSearchMessage.Text = "";

            // Timer to refresh every 3 seconds for real-time HasPendingBorrow
            refreshTimer = new Timer();
            refreshTimer.Interval = 3000; // 3 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();


        }

        private void ManageMembersControl_Load(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                LoadMembers();
            }));
        }

        private void LoadMembers(string searchText = "")
        {
            if (isViewingMember) return; // prevent overwriting during view

            using (var con = Db.GetConnection())
            {
                con.Open();

                string query = @"
            SELECT 
                m.MemberId, 
                m.StudentNo, 
                m.FirstName, 
                m.LastName, 
                m.Course, 
                m.YearLevel,
                CASE WHEN m.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
                CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM Borrowings b 
                        WHERE b.MemberId = m.MemberId AND (b.ReturnDate IS NULL OR b.ReturnDate = '')
                    ) THEN 'Borrowing'
                    ELSE 'No Borrowings'
                END AS HasPendingBorrow
            FROM Members m";

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query += " WHERE (StudentNo LIKE @search OR FirstName LIKE @search OR LastName LIKE @search OR Course LIKE @search)";
                }

                using (var cmd = new SQLiteCommand(query, con))
                {
                    if (!string.IsNullOrWhiteSpace(searchText))
                        cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");

                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // 2️⃣ Bind data
                        dgvMembers.DataSource = dt;

                        dgvMembers.Columns["StudentNo"].HeaderText = "Student Number";
                        dgvMembers.Columns["FirstName"].HeaderText = "First Name";
                        dgvMembers.Columns["LastName"].HeaderText = "Last Name";
                        dgvMembers.Columns["Course"].HeaderText = "Course";
                        dgvMembers.Columns["YearLevel"].HeaderText = "Year Level";
                        dgvMembers.Columns["Status"].HeaderText = "Status";
                        dgvMembers.Columns["HasPendingBorrow"].HeaderText = "Pending Borrow";
                       


                        // 1️⃣ Setup Action button column first
                        SetupActionButtons();

                        // 3️⃣ Hide MemberId
                        if (dgvMembers.Columns.Contains("MemberId"))
                            dgvMembers.Columns["MemberId"].Visible = false;

                        // 4️⃣ Set Action text per row
                        foreach (DataGridViewRow row in dgvMembers.Rows)
                        {
                            bool isActive = row.Cells["Status"].Value.ToString() == "Active";
                            row.Cells["Action"].Value = isActive ? "Deactivate" : "Reactivate";
                        }
                        // Clear selection so no row is pre-selected
                        dgvMembers.ClearSelection();
                        dgvMembers.CurrentCell = null; // also removes focus from any cell

                        // Format rows and add color for HasPendingBorrow
                        foreach (DataGridViewRow row in dgvMembers.Rows)
                        {
                            row.Height = 40;

                            string pending = row.Cells["HasPendingBorrow"].Value.ToString();
                            row.Cells["HasPendingBorrow"].Style.ForeColor =
                                pending == "Borrowing" ? Color.Red : Color.Black;
                        }

                        // ✅ Ensure no row stays selected

                        dgvMembers.CurrentCell = null;




                        lblSearchMessage.Text = dt.Rows.Count == 0 ? "No members match your search." : "";

                    }
                }
            }
        }


        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // 🧭 Remember current selection before refreshing
                int? selectedMemberId = null;
                if (dgvMembers.CurrentRow != null && dgvMembers.CurrentRow.Cells["MemberId"].Value != null)
                    selectedMemberId = Convert.ToInt32(dgvMembers.CurrentRow.Cells["MemberId"].Value);

                using (var con = Db.GetConnection())
                {
                    con.Open();

                    // 🔍 Get the latest borrowing status of all members
                    string query = @"
                SELECT 
                    m.MemberId,
                    CASE 
                        WHEN EXISTS (
                            SELECT 1 
                            FROM Borrowings b 
                            WHERE b.MemberId = m.MemberId 
                            AND (b.ReturnDate IS NULL OR b.ReturnDate = '')
                        ) THEN 'Borrowing'
                        ELSE 'No Borrowings'
                    END AS HasPendingBorrow
                FROM Members m";

                    using (var cmd = new SQLiteCommand(query, con))
                    using (var reader = cmd.ExecuteReader())
                    {
                        var statusDict = new Dictionary<int, string>();
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["MemberId"]);
                            string status = reader["HasPendingBorrow"].ToString();
                            statusDict[id] = status;
                        }

                        // 🧠 Update only the "HasPendingBorrow" column, silently
                        foreach (DataGridViewRow row in dgvMembers.Rows)
                        {
                            if (row.Cells["MemberId"].Value == null) continue;
                            int id = Convert.ToInt32(row.Cells["MemberId"].Value);

                            if (statusDict.TryGetValue(id, out string newStatus))
                            {
                                string oldStatus = row.Cells["HasPendingBorrow"].Value?.ToString();
                                if (oldStatus != newStatus)
                                {
                                    row.Cells["HasPendingBorrow"].Value = newStatus;
                                    row.Cells["HasPendingBorrow"].Style.ForeColor =
                                        newStatus == "Borrowing" ? Color.OrangeRed : Color.Green;
                                }
                            }
                        }
                    }
                }

                // ✅ Restore the previously selected row (keep selection intact)
                if (selectedMemberId.HasValue)
                {
                    foreach (DataGridViewRow row in dgvMembers.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["MemberId"].Value) == selectedMemberId.Value)
                        {
                            row.Selected = true;
                            dgvMembers.CurrentCell = row.Cells[0]; // keeps focus
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RefreshTimer_Tick: " + ex.Message);
            }
        }





        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadMembers(txtSearch.Text.Trim());
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                LoadMembers(txtSearch.Text.Trim());
            }
        }



        private void dgvMembers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return; // skip header

            string colName = dgvMembers.Columns[e.ColumnIndex].Name;

            // Color Edit button
            if (colName == "Edit")
            {
                e.CellStyle.BackColor = Color.LightBlue;      // button background
                e.CellStyle.ForeColor = Color.LightBlue;          // text color
                e.CellStyle.SelectionBackColor = Color.DodgerBlue; // hover selection
                e.CellStyle.Font = new Font(dgvMembers.Font, FontStyle.Regular); // bold text
                e.FormattingApplied = true;
            }
            // Action button (Deactivate = red, Reactivate = green)
            else if (colName == "Action")
            {
                if (e.Value != null)
                {
                    string actionText = e.Value.ToString();
                    if (actionText == "Deactivate")
                    {
                        e.CellStyle.BackColor = Color.IndianRed;
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.SelectionBackColor = Color.Red;
                    }
                    else if (actionText == "Reactivate")
                    {
                        e.CellStyle.BackColor = Color.MediumSeaGreen;
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.SelectionBackColor = Color.Green;
                    }
                }
                e.CellStyle.Font = new Font(dgvMembers.Font, FontStyle.Regular);
                e.FormattingApplied = true;
            }


            if (dgvMembers.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
                e.CellStyle.ForeColor = e.Value.ToString() == "Inactive" ? Color.Red : Color.Green;

            if (dgvMembers.Columns[e.ColumnIndex].Name == "HasPendingBorrow" && e.Value != null)
                e.CellStyle.ForeColor = e.Value.ToString() == "Borrowing" ? Color.Red : Color.Black;

        }

       

        private void dgvMembers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvMembers.Rows[e.RowIndex];
                int memberId = Convert.ToInt32(row.Cells["MemberId"].Value);

                // Always recheck latest borrow status
                string borrowStatus = "No Borrowings";
                string borrowedBooks = "";

                using (var con = Db.GetConnection())
                {
                    con.Open();

                    // Check if the student has active borrowings
                    string statusQuery = @"
                SELECT COUNT(*) 
                FROM Borrowings 
                WHERE MemberId = @id 
                AND (ReturnDate IS NULL OR ReturnDate = '')";
                    using (var cmd = new SQLiteCommand(statusQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", memberId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        borrowStatus = count > 0 ? "Borrowing" : "No Borrowings";
                    }

                    // If borrowing, fetch book titles
                    if (borrowStatus == "Borrowing")
                    {
                        string titleQuery = @"
                    SELECT b.Title
                    FROM Borrowings br
                    JOIN Books b ON br.BookId = b.BookId
                    WHERE br.MemberId = @id 
                    AND (br.ReturnDate IS NULL OR br.ReturnDate = '')";
                        using (var cmd = new SQLiteCommand(titleQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@id", memberId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    borrowedBooks += "• " + reader["Title"].ToString() + "\n";
                            }
                        }
                    }
                }

                // Update grid cell immediately to stay consistent
                row.Cells["HasPendingBorrow"].Value = borrowStatus;
                row.Cells["HasPendingBorrow"].Style.ForeColor =
    borrowStatus == "Borrowing" ? Color.Red : Color.Black;


                // Show message
                string details = $"Student No: {row.Cells["StudentNo"].Value}\n" +
                                 $"Name: {row.Cells["FirstName"].Value} {row.Cells["LastName"].Value}\n" +
                                 $"Course: {row.Cells["Course"].Value}\n" +
                                 $"Year Level: {row.Cells["YearLevel"].Value}\n" +
                                 $"Status: {row.Cells["Status"].Value}\n" +
                                 $"Borrow Status: {borrowStatus}\n\n";

                if (borrowStatus == "Borrowing" && !string.IsNullOrEmpty(borrowedBooks))
                    details += "📚 Borrowed Books:\n" + borrowedBooks;
                else
                    details += "📘 No borrowed books.";

                MessageBox.Show(details, "Member Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }




        private void dgvMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int memberId = Convert.ToInt32(dgvMembers.Rows[e.RowIndex].Cells["MemberId"].Value);

                using (var con = Db.GetConnection())
                {
                    con.Open();
                    string query = @"SELECT b.Title
                                     FROM Borrowings br
                                     JOIN Books b ON br.BookId = b.BookId
                                     WHERE br.MemberId = @id AND (br.ReturnDate IS NULL OR br.ReturnDate = '')";

                    using (var cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", memberId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            string borrowedBooks = "";
                            while (reader.Read())
                                borrowedBooks += "- " + reader["Title"].ToString() + "\n";

                            if (string.IsNullOrEmpty(borrowedBooks))
                                borrowedBooks = "No pending borrowed books.";

                            MessageBox.Show(borrowedBooks, "Borrowed Books");
                        }
                    }
                }
            }
        }


        private void SetupActionButtons()
        {
            // Add Edit column if it doesn't exist
            if (!dgvMembers.Columns.Contains("Edit"))
            {
                DataGridViewButtonColumn editCol = new DataGridViewButtonColumn
                {
                    Name = "Edit",
                    HeaderText = "",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true,
                    
                };
                dgvMembers.Columns.Add(editCol);
                editCol.Width = 70;
            }

            // Add Action column (Deactivate/Reactivate) if it doesn't exist
            if (!dgvMembers.Columns.Contains("Action"))
            {
                DataGridViewButtonColumn actionCol = new DataGridViewButtonColumn
                {
                    Name = "Action",
                    HeaderText = "",
                    UseColumnTextForButtonValue = false, // text will change per row
                   
                };
                dgvMembers.Columns.Add(actionCol);
                actionCol.Width = 70;
            }

            // Make sure they are the last columns
            dgvMembers.Columns["Edit"].DisplayIndex = dgvMembers.Columns.Count - 2;
            dgvMembers.Columns["Action"].DisplayIndex = dgvMembers.Columns.Count - 1;
        }



        private void DgvMembers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // ignore header
            var row = dgvMembers.Rows[e.RowIndex];

            int memberId = Convert.ToInt32(row.Cells["MemberId"].Value);

            if (dgvMembers.Columns[e.ColumnIndex].Name == "Edit")
            {
                string firstName = row.Cells["FirstName"].Value.ToString();
                string lastName = row.Cells["LastName"].Value.ToString();
                string course = row.Cells["Course"].Value.ToString();
                string yearLevel = row.Cells["YearLevel"].Value.ToString();

                MemberEditForm editForm = new MemberEditForm(memberId, firstName, lastName, course, yearLevel);
                if (editForm.ShowDialog() == DialogResult.OK)
                    LoadMembers(txtSearch.Text.Trim());
            }
            else if (dgvMembers.Columns[e.ColumnIndex].Name == "Action")
            {
                string currentAction = row.Cells["Action"].Value?.ToString();
                if (currentAction == "Deactivate")
                    DeactivateMember(memberId, row);
                else if (currentAction == "Reactivate")
                    ReactivateMember(memberId, row);
            }
        }

        private void DeactivateMember(int memberId, DataGridViewRow row)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM Borrowings WHERE MemberId = @memberId AND ReturnDate IS NULL";
                using (var cmd = new SQLiteCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@memberId", memberId);
                    int activeBorrows = Convert.ToInt32(cmd.ExecuteScalar());
                    if (activeBorrows > 0)
                    {
                        MessageBox.Show(
                            "This member cannot be deactivated because they still have borrowed books.",
                            "Action Denied",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }
                }

                using (var cmd = new SQLiteCommand("UPDATE Members SET IsActive = 0 WHERE MemberId = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member deactivated successfully!");

            // Update buttons for this row only
            row.Cells["Status"].Value = "Inactive";
            row.Cells["Action"].Value = "Reactivate";

        }


        private void ReactivateMember(int memberId, DataGridViewRow row)
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                using (var cmd = new SQLiteCommand("UPDATE Members SET IsActive = 1 WHERE MemberId = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", memberId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Member reactivated successfully!");

            // Update buttons for this row only
            row.Cells["Status"].Value = "Active";
            row.Cells["Action"].Value = "Deactivate";

        }


        private void dgvMembers_CellPainting_HeaderAction(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0 && e.ColumnIndex < dgvMembers.Columns.Count)
            {
                string colName = dgvMembers.Columns[e.ColumnIndex].Name;
                if (colName == "Edit" || colName == "Deactivate")
                    e.Handled = true;
            }
        }

        private void DgvMembers_Paint_ActionHeader(object sender, PaintEventArgs e)
        {
            if (!dgvMembers.Columns.Contains("Edit") || !dgvMembers.Columns.Contains("Action")) return;

            // Get rectangles for the two button columns
            Rectangle editRect = dgvMembers.GetCellDisplayRectangle(dgvMembers.Columns["Edit"].Index, -1, true);
            Rectangle actionRect = dgvMembers.GetCellDisplayRectangle(dgvMembers.Columns["Action"].Index, -1, true);

            // Combine the rectangles into one header area
            Rectangle headerRect = new Rectangle(editRect.X, editRect.Y, actionRect.X + actionRect.Width - editRect.X, editRect.Height);

            // Draw background and border
            e.Graphics.FillRectangle(Brushes.LightGray, headerRect);
            e.Graphics.DrawRectangle(Pens.Gray, headerRect);

            // Draw header text centered
            TextRenderer.DrawText(
                 e.Graphics,
                 "Action",
                 new Font("Segoe UI", 10, FontStyle.Regular),   // ★ custom font here
                 headerRect,
                 Color.Black,
                 TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
             );

        }

        private void dgvMembers_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
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