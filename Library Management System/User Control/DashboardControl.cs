using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Printing;


namespace Library_Management_System.User_Control
{
    public partial class DashboardControl : UserControl
    {
        private Timer _greetingTimer;
        private Timer _statsTimer;
        private string _userDisplayName;
        private Panel pnlChartContainer;
        private Chart chartReports;
        private Button btnToday;
        private Button btnLastWeek;
        private Button btnLastMonth;
        private string _currentTrendType = "borrowed";
        private Panel pnlUnderline; // Add this at the top with your fields
        private Panel pnlTrendUnderline;
        private Timer _chartRefreshTimer;
        private string _currentTimeRange = "today";




        public DashboardControl(string userDisplayName)
        {
            InitializeComponent();
            _userDisplayName = string.IsNullOrWhiteSpace(userDisplayName) ? "ADMIN" : userDisplayName;

            // Initialize greeting
            UpdateGreeting();

            // Greeting timer
            _greetingTimer = new Timer { Interval = 60 * 1000 };
            _greetingTimer.Tick += (s, e) => UpdateGreeting();
            _greetingTimer.Start();

            // Load stats
            LoadDashboardStats();

            // Auto-refresh stats every 10 seconds
            _statsTimer = new Timer { Interval = 10 * 1000 };
            _statsTimer.Tick += (s, e) => LoadDashboardStats();
            _statsTimer.Start();

            // Wire label clicks
            lblTotalBooks.Click += pnlTotalBooks_Click;
            lblActiveMembers.Click += pnlActiveMembers_Click;
            lblBorrowedBooks.Click += (s, e) => LoadDashboardStats();



        }

        private void SetupDashboardChart()
        {
            // --- Chart Container Panel ---
            pnlChartContainer = new Panel
            {
                Size = new Size(1280, 352),
                BackColor = Color.White,
                Location = new Point(225, 230),
            };
            ApplyCardStyle(pnlChartContainer, 12);


            // --- Reports Label ---
            Label lblReports = new Label
            {
                Text = "Reports",
                Font = new Font("Microsoft Sans Serif", 14, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(20, 5) // small top margin
            };
            pnlChartContainer.Controls.Add(lblReports);
            lblReports.BringToFront();



            // 🔹 Report Type Buttons (Borrowed / Returned / Overdue)
            Button btnBorrowedTrend = new Button
            {
                Text = "Borrowed",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,  // remove blue
                ForeColor = Color.Black,
                Size = new Size(90, 28),
                Location = new Point(350, 5),
                Cursor = Cursors.Hand
            };
            btnBorrowedTrend.FlatAppearance.BorderSize = 0;

            Button btnReturnedTrend = new Button
            {
                Text = "Returned",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,  // remove blue
                ForeColor = Color.Black,        // normal text
                Size = new Size(90, 28),
                Location = new Point(450, 5),
                Cursor = Cursors.Hand
            };
            btnReturnedTrend.FlatAppearance.BorderSize = 0;

            Button btnOverdueTrend = new Button
            {
                Text = "Overdue",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,  // remove blue
                ForeColor = Color.Black,        // normal text
                Size = new Size(90, 28),
                Location = new Point(550, 5),
                Cursor = Cursors.Hand
            };
            btnOverdueTrend.FlatAppearance.BorderSize = 0;

            // Add them to panel
            pnlChartContainer.Controls.Add(btnBorrowedTrend);
            pnlChartContainer.Controls.Add(btnReturnedTrend);
            pnlChartContainer.Controls.Add(btnOverdueTrend);



            // --- Tabs (Buttons) ---
            btnToday = new Button
            {
                Text = "Today",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,  // remove blue
                ForeColor = Color.Black,        // normal text
                Size = new Size(80, 28),
                Location = new Point(20, 5),
                Cursor = Cursors.Hand
            };
            btnToday.FlatAppearance.BorderSize = 0;

            btnLastWeek = new Button
            {
                Text = "Last Week",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,  // remove blue
                ForeColor = Color.Black,        // normal text
                Size = new Size(100, 28),
                Location = new Point(110, 5),
                Cursor = Cursors.Hand
            };
            btnLastWeek.FlatAppearance.BorderSize = 0;

            btnLastMonth = new Button
            {
                Text = "Last Month",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,  // remove blue
                ForeColor = Color.Black,        // normal text
                Size = new Size(110, 28),
                Location = new Point(220, 5),
                Cursor = Cursors.Hand
            };
            btnLastMonth.FlatAppearance.BorderSize = 0;

            // Trend buttons (Borrowed / Returned / Overdue)
            btnBorrowedTrend.Location = new Point(20, lblReports.Bottom + 5);
            btnReturnedTrend.Location = new Point(btnBorrowedTrend.Right + 5, lblReports.Bottom + 5);
            btnOverdueTrend.Location = new Point(btnReturnedTrend.Right + 5, lblReports.Bottom + 5);


            btnBorrowedTrend.Click += (s, e) => {
                _currentTrendType = "borrowed";
                HighlightTrendButton(btnBorrowedTrend, btnReturnedTrend, btnOverdueTrend);
                LoadChartData(_currentTimeRange); // Use currently selected time range
            };

            btnReturnedTrend.Click += (s, e) => {
                _currentTrendType = "returned";
                HighlightTrendButton(btnReturnedTrend, btnBorrowedTrend, btnOverdueTrend);
                LoadChartData(_currentTimeRange); // Use currently selected time range
            };

            btnOverdueTrend.Click += (s, e) => {
                _currentTrendType = "overdue";
                HighlightTrendButton(btnOverdueTrend, btnBorrowedTrend, btnReturnedTrend);
                LoadChartData(_currentTimeRange);
            };

            // Top padding below the label
            int topOffset = lblReports.Bottom + 5;
            int rightPadding = 20; // distance from the right edge
            int spacing = 10;      // space between buttons

            // Position buttons from right to left
            btnLastMonth.Location = new Point(pnlChartContainer.Width - rightPadding - btnLastMonth.Width, topOffset);
            btnLastWeek.Location = new Point(btnLastMonth.Left - spacing - btnLastWeek.Width, topOffset);
            btnToday.Location = new Point(btnLastWeek.Left - spacing - btnToday.Width, topOffset);


            // --- Chart ---
            chartReports = new Chart
            {
                Dock = DockStyle.Bottom,
                Height = 240,
                BackColor = Color.White,
                BorderlineColor = Color.Transparent
            };

            ChartArea area = new ChartArea("MainArea");
            chartReports.ChartAreas.Add(area);

            area.BackColor = Color.White;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisX.LineWidth = 0;
            area.AxisY.LineWidth = 0;
            area.AxisX.LabelStyle.ForeColor = Color.Black;
            area.AxisY.LabelStyle.ForeColor = Color.Black;
            area.AxisY.Minimum = 0;

            Series series = new Series("Reports")
            {
                ChartType = SeriesChartType.SplineArea,
                Color = Color.FromArgb(120, 210, 180, 140),   // light brown fill with transparency 
                BorderWidth = 3,
                BorderColor = Color.SandyBrown,               // solid light brown line
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8,
                MarkerColor = Color.White,
                MarkerBorderColor = Color.SandyBrown,  // border color same as line
                MarkerBorderWidth = 2
            };
            chartReports.Series.Add(series);

            LoadChartData("today");

            // --- Button Clicks ---
            btnToday.Click += (s, e) => {
                _currentTimeRange = "today";
                MoveUnderline(btnToday);
                HighlightButton(btnToday);
                LoadChartData(_currentTimeRange);
            };

            btnLastWeek.Click += (s, e) => {
                _currentTimeRange = "week";
                MoveUnderline(btnLastWeek);
                HighlightButton(btnLastWeek);
                LoadChartData(_currentTimeRange);
            };

            btnLastMonth.Click += (s, e) => {
                _currentTimeRange = "month";
                MoveUnderline(btnLastMonth);
                HighlightButton(btnLastMonth);
                LoadChartData(_currentTimeRange);
            };


            // --- Add to Container ---
            pnlChartContainer.Controls.Add(btnToday);
            pnlChartContainer.Controls.Add(btnLastWeek);
            pnlChartContainer.Controls.Add(btnLastMonth);
            pnlChartContainer.Controls.Add(chartReports);

            // Create underline panel
            pnlUnderline = new Panel
            {
                Size = new Size(btnToday.Width, 3), // height of the underline
                BackColor = Color.SandyBrown, // light brown
                Location = new Point(btnToday.Left, btnToday.Bottom) // under the selected tab
            };
            pnlChartContainer.Controls.Add(pnlUnderline);
            pnlUnderline.BringToFront(); // make sure it appears above other controls

            // --- button click events ---
            btnToday.Click += (s, e) => { MoveUnderline(btnToday); HighlightButton(btnToday); LoadChartData("today"); };
            btnLastWeek.Click += (s, e) => { MoveUnderline(btnLastWeek); HighlightButton(btnLastWeek); LoadChartData("week"); };
            btnLastMonth.Click += (s, e) => { MoveUnderline(btnLastMonth); HighlightButton(btnLastMonth); LoadChartData("month"); };

            // Set initial underline position
            MoveUnderline(btnToday);

            // Create trend underline panel
            pnlTrendUnderline = new Panel
            {
                Size = new Size(btnBorrowedTrend.Width, 3), // height of the underline
                BackColor = Color.SandyBrown,
                Location = new Point(btnBorrowedTrend.Left, btnBorrowedTrend.Bottom) // under the selected trend
            };
            pnlChartContainer.Controls.Add(pnlTrendUnderline);
            pnlTrendUnderline.BringToFront(); // make sure it appears above other controls

            // 🔁 Auto-refresh chart every 30 seconds
            _chartRefreshTimer = new Timer { Interval = 30 * 1000 };
            _chartRefreshTimer.Tick += (s, m) => LoadChartData(_currentTimeRange);
            _chartRefreshTimer.Start();


            this.Controls.Add(pnlChartContainer);

            Button btnExport = new Button
            {
                Text = "Export",
                Location = new Point(20, chartReports.Top - 30),
                Size = new Size(100, 30)
            };
            btnExport.Click += (s, e) => ExportChartAsImage();
            pnlChartContainer.Controls.Add(btnExport);

            Button btnPrint = new Button
            {
                Text = "Print",
                Location = new Point(130, chartReports.Top - 30),
                Size = new Size(100, 30)
            };
            btnPrint.Click += (s, e) => PrintChart();
            pnlChartContainer.Controls.Add(btnPrint);
        }

        private void LoadChartData(string mode)
        {
            Series series = chartReports.Series["Reports"];
            series.Points.Clear();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string dateField = "BorrowDate";
                string chartTitle = "📚 Borrowing Activity Over Time";

                switch (_currentTrendType)
                {
                    case "returned":
                        dateField = "ReturnDate";
                        chartTitle = "🔁 Returns Over Time";
                        break;
                    case "overdue":
                        dateField = "DueDate";
                        chartTitle = "⚠ Overdue Count Trend";
                        break;
                }


                string query = "";

                // --- Construct query based on mode ---
                if (mode == "today")
                {

                    if (_currentTrendType == "returned")
                    {
                        // Query for returns in the current day (7 AM to 5 PM)
                        query = @"
                        SELECT strftime('%H', ReturnDate) AS Label, COUNT(*) AS Count
                        FROM Borrowings
                        WHERE ReturnDate IS NOT NULL
                        AND strftime('%H', ReturnDate) BETWEEN '07' AND '17'  -- Filter for library open hours (7 AM to 5 PM)
                        GROUP BY Label
                        ORDER BY Label;
                        ";
                    }
                    else if (_currentTrendType == "overdue")
                    {
                        query = @"
                        SELECT strftime('%H', DueDate) AS Label, COUNT(*) AS Count
                        FROM Borrowings
                        WHERE ReturnDate IS NULL AND DATE(DueDate) <= DATE('now')
                        AND strftime('%H', DueDate) BETWEEN '07' AND '17'  -- Filter for overdue books within library working hours
                        GROUP BY Label
                        ORDER BY Label;
                        ";
                    }
                    else if (_currentTrendType == "borrowed")
                    {
                        query = $@"
                        SELECT strftime('%H', {dateField}) AS Label, COUNT(*) AS Count
                        FROM Borrowings
                        WHERE DATE({dateField}) = DATE('now')
                        AND strftime('%H', {dateField}) BETWEEN '07' AND '17'  -- Filter for library open hours (7 AM to 5 PM)
                        GROUP BY Label ORDER BY Label;
                        ";
                    }
                }
                else if (mode == "week")
                {
                        query = $@"
                        SELECT strftime('%w', {dateField}) AS Label, COUNT(*) AS Count
                        FROM Borrowings
                        WHERE DATE({dateField}) >= DATE('now', '-6 day')
                        {(_currentTrendType == "overdue" ? "AND ReturnDate IS NULL AND DueDate <= DATE('now')" : "")}
                        GROUP BY Label
                        ORDER BY Label;
                    ";
                }
                else if (mode == "month")
                {
                        query = $@"
                        SELECT strftime('%d', {dateField}) AS Label, COUNT(*) AS Count
                        FROM Borrowings
                        WHERE strftime('%Y-%m', {dateField}) = strftime('%Y-%m', 'now')
                        {(_currentTrendType == "overdue" ? "AND ReturnDate IS NULL AND DueDate <= DATE('now')" : "")}
                        GROUP BY Label
                        ORDER BY Label;
                    ";
                }

                Console.WriteLine(query);


                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    if (mode == "week")
                    {
                        string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                        int[] values = new int[7]; // prefill with 0

                        while (reader.Read())
                        {
                            int dayIndex = int.Parse(reader["Label"].ToString());
                            if (dayIndex >= 0 && dayIndex < 7)
                                values[dayIndex] = Convert.ToInt32(reader["Count"]);
                        }

                        for (int i = 0; i < 7; i++)
                            series.Points.AddXY(days[i], values[i]);
                    }
                    else if (mode == "month")
                    {
                        int daysInMonth = DateTime.Now.Day; // show until today
                        int[] values = new int[daysInMonth];

                        while (reader.Read())
                        {
                            int day = int.Parse(reader["Label"].ToString()) - 1; // 1-based to 0-based
                            if (day >= 0 && day < daysInMonth)
                                values[day] = Convert.ToInt32(reader["Count"]);
                        }

                        for (int i = 0; i < daysInMonth; i++)
                            series.Points.AddXY((i + 1).ToString(), values[i]);
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            string label = reader["Label"].ToString();
                            double value = Convert.ToDouble(reader["Count"]);
                            series.Points.AddXY(label, value);
                        }
                    }
                }

                chartReports.Titles.Clear();
                chartReports.Titles.Add(chartTitle);
                chartReports.Titles[0].Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold);
                chartReports.Titles[0].ForeColor = Color.Black;
            }

            foreach (DataPoint point in series.Points)
                point.ToolTip = $"{point.AxisLabel}: {point.YValues[0]}";

            chartReports.Invalidate();
        }









        // Method to move underline under the active button
        private void MoveUnderline(Button activeButton)
        {
            pnlUnderline.Left = activeButton.Left;
            pnlUnderline.Width = activeButton.Width;
        }

        private void HighlightTrendButton(Button active, Button b1, Button b2)
        {
            // Reset all text color to black
            foreach (var btn in new[] { active, b1, b2 })
            {
                btn.ForeColor = Color.Black;
            }

            // Move underline to active
            pnlTrendUnderline.Left = active.Left;
            pnlTrendUnderline.Width = active.Width;
        }

        private void HighlightButton(Button activeButton)
        {
            // Only move underline, no button color change
            MoveUnderline(activeButton);

            foreach (var btn in new[] { btnToday, btnLastWeek, btnLastMonth })
            {
                btn.ForeColor = Color.Black; // all text gray
            }

            activeButton.ForeColor = Color.Black; // optional: active text slightly darker
        }
        public DashboardControl() : this("ADMIN") { }

        private void DashboardControl_Load(object sender, EventArgs e)
        {

            SetupDashboardChart();
            // Light dashboard background
            //this.BackColor = ColorTranslator.FromHtml("#f5f6fa");

            // Apply distinct colors for each card
            StyleCard(pnlTotalBooks, Color.White);   // light blue
            StyleCard(pnlBorrowedBooks, Color.White); // light red
            StyleCard(pnlAvailableBooks, Color.White); // light green
            StyleCard(pnlActiveMembers, Color.White);  // light yellow
            StyleCard(pnlOverdueBooks, Color.White);     // orange red

            // Rounded corners
            ApplyCardStyle(pnlTotalBooks, 10);
            ApplyCardStyle(pnlBorrowedBooks, 10);
            ApplyCardStyle(pnlAvailableBooks, 10);
            ApplyCardStyle(pnlActiveMembers, 10);
            ApplyCardStyle(pnlOverdueBooks, 10);

            // Example (assuming you have picture boxes for each panel)
            ApplyPictureBoxStyle(picTotalBooks, 2);
            ApplyPictureBoxStyle(picBorrowedBooks, 2);
            ApplyPictureBoxStyle(picAvailableBooks, 2);
            ApplyPictureBoxStyle(picActiveMembers, 2);
            ApplyPictureBoxStyle(picOverdueBooks, 2);




            // Remove extra margins to save space
            foreach (Panel pnl in new[] { pnlTotalBooks, pnlBorrowedBooks, pnlAvailableBooks, pnlActiveMembers, pnlOverdueBooks })
            {
                pnl.Margin = new Padding(8);
            }
        }



        private void ExportChartAsImage()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                Title = "Save Chart as Image"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                chartReports.SaveImage(sfd.FileName, ChartImageFormat.Png);
                MessageBox.Show("Chart exported successfully!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void PrintChart()
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (s, e) =>
            {
                Bitmap bmp = new Bitmap(chartReports.Width, chartReports.Height);
                chartReports.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                e.Graphics.DrawImage(bmp, new Point(50, 50));
            };

            PrintPreviewDialog dlg = new PrintPreviewDialog
            {
                Document = pd,
                Width = 1000,
                Height = 800
            };
            dlg.ShowDialog();
        }


        private void StyleCard(Panel panel, Color bgColor)
        {
            panel.BackColor = bgColor;
            //panel.Cursor = Cursors.Hand;

            // Hover effect
            panel.MouseEnter += (s, e) => panel.BackColor = ControlPaint.Light(bgColor, 0.2f);
            panel.MouseLeave += (s, e) => panel.BackColor = bgColor;

            //foreach (Control ctrl in panel.Controls)
            //{
            //    if (ctrl is Label lbl)
            //    {
            //        lbl.ForeColor = Color.Black;
            //        lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            //    }
            //}
        }



        // ---------------------- STYLING ----------------------

        private void ApplyCardStyle(Panel panel, int radius)
        {
            panel.Padding = new Padding(10);
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                Rectangle rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);

                using (GraphicsPath path = new GraphicsPath())
                {
                    int diameter = radius * 2;
                    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                    path.CloseFigure();

                    // ✅ Use panel's existing BackColor (don’t overwrite!)
                    using (SolidBrush brush = new SolidBrush(panel.BackColor))
                        e.Graphics.FillPath(brush, path);

                    // Soft border for a subtle shadow look
                    using (Pen borderPen = new Pen(Color.FromArgb(220, 220, 220), 2))
                        e.Graphics.DrawPath(borderPen, path);
                }

                // Apply rounded clipping region
                using (GraphicsPath clipPath = new GraphicsPath())
                {
                    int diameter = radius * 2;
                    clipPath.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                    clipPath.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                    clipPath.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                    clipPath.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                    clipPath.CloseFigure();
                    panel.Region = new Region(clipPath);
                }
            };
        }


        // ---------------------- GREETING ----------------------

        private void UpdateGreeting()
        {
            string period = GetTimePeriod(DateTime.Now);
            lblGreeting.Text = $"Good {period}, {_userDisplayName.ToUpper()}!";
        }

        private string GetTimePeriod(DateTime dt)
        {
            int hour = dt.Hour;
            if (hour >= 5 && hour < 12) return "morning";
            else if (hour >= 12 && hour < 17) return "afternoon";
            else if (hour >= 17 && hour < 21) return "evening";
            else return "night";
        }

        // ---------------------- DASHBOARD DATA ----------------------

        private void LoadDashboardStats()
        {
            using (var con = Db.GetConnection())
            {
                con.Open();

                int totalBooks = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Books", con).ExecuteScalar());
                int borrowedBooks = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Borrowings WHERE ReturnDate IS NULL", con).ExecuteScalar());
                int availableBooks = totalBooks - borrowedBooks;
                int activeMembers = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Members WHERE IsActive = 1", con).ExecuteScalar());
                int overdueBooks = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM Borrowings WHERE ReturnDate IS NULL AND DueDate < DATE('now')", con).ExecuteScalar());

                lblTotalBooks.Text = $"Total Books";
                
                lblBorrowedBooks.Text = $"Borrowed";
                lblAvailableBooks.Text = $"Available";
                lblActiveMembers.Text = $"Active Members";
                lblOverdueBooks.Text = $"Overdue";

                // Value labels (under the title, bottom-right aligned)
                lblTotalBooksValue.Text = totalBooks.ToString();
                lblBorrowedBooksValue.Text = borrowedBooks.ToString();
                lblAvailableBooksValue.Text = availableBooks.ToString();
                lblActiveMembersValue.Text = activeMembers.ToString();
                lblOverdueBooksValue.Text = overdueBooks.ToString();
            }
        }

        // ---------------------- EVENTS ----------------------

        private void lblBorrowedBooks_Click(object sender, EventArgs e)
        {
            UpdateBorrowedBooksCount();
        }

        private void pnlTotalBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            parentForm?.OpenManageBooksFromDashboard();
        }

        private void pnlActiveMembers_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            parentForm?.OpenManageMembersFromDashboard();
        }

        private void pnlBorrowedBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            parentForm?.LoadControl(new BorrowBooksControl());
        }

        private void pnlAvailableBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            parentForm?.LoadControl(new BooksForm());
        }

        private void pnlOverdueBooks_Click(object sender, EventArgs e)
        {
            MainForm parentForm = this.FindForm() as MainForm;
            parentForm?.LoadControl(new OverdueReportControl());
        }

        private void UpdateBorrowedBooksCount()
        {
            try
            {
                var dt = DatabaseHelper.Query("SELECT COUNT(*) AS cnt FROM Borrowings WHERE ReturnDate IS NULL");
                int borrowedCount = Convert.ToInt32(dt.Rows[0]["cnt"]);
                lblBorrowedBooks.Text = $"📖 Borrowed Books: {borrowedCount}";
            }
            catch
            {
                lblBorrowedBooks.Text = "📖 Borrowed Books: 0";
            }
        }

        public void RefreshStats() => LoadDashboardStats();

        private void lblTotalBooks_Click(object sender, EventArgs e)
        {
          
        }

        private void lblAvailableBooks_Click(object sender, EventArgs e)
        {

        }

        private void lblActiveMembers_Click(object sender, EventArgs e)
        {
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            _greetingTimer?.Stop();
            _greetingTimer?.Dispose();
            _statsTimer?.Stop();
            _statsTimer?.Dispose();
            base.OnHandleDestroyed(e);
        }

        private void panel1_Paint(object sender, PaintEventArgs e){ }

        private void ApplyPictureBoxStyle(PictureBox pictureBox, int radius)
        {
            pictureBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int shadowOffset = 4;
                int shadowBlur = 6;

                Rectangle rect = new Rectangle(0, 0, pictureBox.Width - 1, pictureBox.Height - 1);

                using (GraphicsPath path = new GraphicsPath())
                {
                    int d = radius * 2;
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();

                    // ---- Draw Outside Shadow ----
                    using (GraphicsPath shadowPath = new GraphicsPath())
                    {
                        Rectangle shadowRect = new Rectangle(
                            rect.X + shadowOffset,
                            rect.Y + shadowOffset,
                            rect.Width,
                            rect.Height
                        );

                        // Clip to rounded shape
                        pictureBox.Region = new Region(path);
                    }

                    // Optional border
                    using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };
        }
        private void pnlActiveMembers_Paint(object sender, PaintEventArgs e){}
        private void label3_Click(object sender, EventArgs e){}
        private void pnlTotalBooks_Paint(object sender, PaintEventArgs e){}
        private void label2_Click(object sender, EventArgs e){}
        private void label2_Click_1(object sender, EventArgs e){}
        private void label3_Click_1(object sender, EventArgs e){}
        private void label4_Click(object sender, EventArgs e){}
        private void label5_Click(object sender, EventArgs e){}
        private void label6_Click(object sender, EventArgs e){}
        private void pnlTotalBooks_Paint_1(object sender, PaintEventArgs e){}
        private void pictureBox1_Click(object sender, EventArgs e){}
        private void pictureBox6_Click(object sender, EventArgs e){}
        private void picAvailableBooks_Click(object sender, EventArgs e){}
        private void picActiveMembers_Click(object sender, EventArgs e) { }
        private void picOverdueBooks_Click(object sender, EventArgs e){ }
        private void lblGreeting_Click(object sender, EventArgs e){ }
    }
}
