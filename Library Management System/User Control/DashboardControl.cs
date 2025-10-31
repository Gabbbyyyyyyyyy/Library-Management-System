﻿using System;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;
using System.Windows.Forms.DataVisualization.Charting;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Data;
using System.Linq;


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
        private Panel pnlUnderline; // Add this at the top with your fields
        private string _currentTimeRange = "today";
        private string currentTrend = "Borrowings";
        private Button activeTrendButton;
        private Button btnBorrowings, btnReturns, btnOverdue;
        private Label lblRecentBorrowings;
        private DataGridView dgvRecentBorrowings;
        private Panel pnlRecentBorrowings;
        private Timer penaltyTimer; // ⏱ Timer to auto-refresh penalty data

        public DashboardControl(string userDisplayName)
        {
            InitializeComponent();
            InitializeRecentBorrowingsSection();
            LoadRecentBorrowings();
            LoadMostBorrowedBooks();

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
            _statsTimer.Tick += (s, e) =>
            {
                LoadDashboardStats();
                LoadChartData(_currentTimeRange);
                //// 🟢 Add this line to refresh the penalty summary dynamically
                //RefreshPenaltySummary();
            };

            _statsTimer.Start();

            // Wire label clicks
            lblTotalBooks.Click += pnlTotalBooks_Click;
            lblActiveMembers.Click += pnlActiveMembers_Click;
            lblBorrowedBooks.Click += (s, e) => LoadDashboardStats();
        }

        // Method to move underline under the active button
        private void MoveUnderline(Button activeButton)
        {
            pnlUnderline.Left = activeButton.Left;
            pnlUnderline.Width = activeButton.Width;
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

        private void ToggleTrendButton(Button activeButton)
        {
            // Reset all
            btnBorrowings.ForeColor = Color.Black;
            btnReturns.ForeColor = Color.Black;
            btnOverdue.ForeColor = Color.Black;

            // Highlight active
            activeButton.ForeColor = Color.FromArgb(205, 133, 63); // Peru highlight color
        }
        private void SetupDashboardChart()
        {
            // --- Chart Container ---
            pnlChartContainer = new Panel
            {
                Size = new Size(1280, 325),
                BackColor = Color.White,
                Location = new Point(226, 215)
            };
            ApplyCardStyle(pnlChartContainer, 12);

            // --- Reports Label ---
            Label lblReports = new Label
            {
                Text = "Borrowing Activity",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 50, 20),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            pnlChartContainer.Controls.Add(lblReports);

            // --- Time Range Tabs ---
            btnToday = new Button
            {
                Text = "Today",
                Font = new Font("Microsoft Sans Serif", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = new Size(80, 28),
                Cursor = Cursors.Hand
            };
            btnToday.FlatAppearance.BorderSize = 0;

            btnLastWeek = new Button
            {
                Text = "Last Week",
                Font = new Font("Microsoft Sans Serif", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = new Size(100, 28),
                Cursor = Cursors.Hand
            };
            btnLastWeek.FlatAppearance.BorderSize = 0;

            btnLastMonth = new Button
            {
                Text = "Last Month",
                Font = new Font("Microsoft Sans Serif", 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = new Size(110, 28),
                Cursor = Cursors.Hand
            };
            btnLastMonth.FlatAppearance.BorderSize = 0;

            int topOffset = lblReports.Bottom - 20;
            int rightPadding = 200;
            int spacing = 10;

            btnLastMonth.Location = new Point(pnlChartContainer.Width - rightPadding - btnLastMonth.Width, topOffset);
            btnLastWeek.Location = new Point(btnLastMonth.Left - spacing - btnLastWeek.Width, topOffset);
            btnToday.Location = new Point(btnLastWeek.Left - spacing - btnToday.Width, topOffset);

            pnlChartContainer.Controls.Add(btnToday);
            pnlChartContainer.Controls.Add(btnLastWeek);
            pnlChartContainer.Controls.Add(btnLastMonth);

            // --- Trend Toggle Buttons (Borrowing / Returns / Overdue) ---
            btnBorrowings = new Button
            {
                Text = "Borrowings",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(165, 105, 79), // Coffee brown - active by default
                ForeColor = Color.White,
                Size = new Size(100, 28),
                Cursor = Cursors.Hand
            };
            btnBorrowings.FlatAppearance.BorderSize = 0;

            btnReturns = new Button
            {
                Text = "Returns",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = new Size(100, 28),
                Cursor = Cursors.Hand
            };
            btnReturns.FlatAppearance.BorderSize = 0;

            btnOverdue = new Button
            {
                Text = "Overdue",
                Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Size = new Size(100, 28),
                Cursor = Cursors.Hand
            };
            btnOverdue.FlatAppearance.BorderSize = 0;

            // Position trend buttons under the time range
            int trendTop = btnToday.Bottom + 10;
            btnBorrowings.Location = new Point(20, trendTop);
            btnReturns.Location = new Point(btnBorrowings.Right + 10, trendTop);
            btnOverdue.Location = new Point(btnReturns.Right + 10, trendTop);

            pnlChartContainer.Controls.Add(btnBorrowings);
            pnlChartContainer.Controls.Add(btnReturns);
            pnlChartContainer.Controls.Add(btnOverdue);

            // --- Trend Toggle Events ---
            btnBorrowings.Click += (s, e) =>
            {
                ToggleTrendButton(btnBorrowings);
                chartReports.Series["Borrowing Activity"].Enabled = btnBorrowings.BackColor != Color.White;
            };

            btnReturns.Click += (s, e) =>
            {
                ToggleTrendButton(btnReturns);
                chartReports.Series["Returns"].Enabled = btnReturns.BackColor != Color.White;
            };

            btnOverdue.Click += (s, e) =>
            {
                ToggleTrendButton(btnOverdue);
                chartReports.Series["Overdue"].Enabled = btnOverdue.BackColor != Color.White;
            };
            // --- Chart ---
            chartReports = new Chart
            {
                Dock = DockStyle.Bottom,
                Height = 240,
                BackColor = Color.White
            };

            ChartArea area = new ChartArea("MainArea");
            chartReports.ChartAreas.Add(area);

            area.BackColor = Color.White;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisX.LineWidth = 1;
            area.AxisY.LineWidth = 1;
            area.AxisX.LabelStyle.ForeColor = Color.Black;
            area.AxisY.LabelStyle.ForeColor = Color.Black;
            area.AxisY.Minimum = 0;

            pnlChartContainer.Controls.Add(chartReports);
            this.Controls.Add(pnlChartContainer);

            // --- Underline (for time range) ---
            pnlUnderline = new Panel
            {
                Size = new Size(btnToday.Width, 3),
                BackColor = Color.FromArgb(205, 133, 63),
                Location = new Point(btnToday.Left, btnToday.Bottom)
            };
            pnlChartContainer.Controls.Add(pnlUnderline);
            pnlUnderline.BringToFront();

            // --- Events ---
            btnToday.Click += (s, e) => { _currentTimeRange = "today"; MoveUnderline(btnToday); HighlightButton(btnToday); LoadChartData(_currentTimeRange, currentTrend); };
            btnLastWeek.Click += (s, e) => { _currentTimeRange = "week"; MoveUnderline(btnLastWeek); HighlightButton(btnLastWeek); LoadChartData(_currentTimeRange, currentTrend); };
            btnLastMonth.Click += (s, e) => { _currentTimeRange = "month"; MoveUnderline(btnLastMonth); HighlightButton(btnLastMonth); LoadChartData(_currentTimeRange, currentTrend); };

            btnBorrowings.Click += (s, e) => { currentTrend = "Borrowings"; HighlightTrendButton(btnBorrowings); LoadChartData(_currentTimeRange, currentTrend); };
            btnReturns.Click += (s, e) => { currentTrend = "Returns"; HighlightTrendButton(btnReturns); LoadChartData(_currentTimeRange, currentTrend); };
            btnOverdue.Click += (s, e) => { currentTrend = "Overdue"; HighlightTrendButton(btnOverdue); LoadChartData(_currentTimeRange, currentTrend); };

            // --- Load initial ---
            _currentTimeRange = "today";
            currentTrend = "Borrowings";
            HighlightButton(btnToday);
            HighlightTrendButton(btnBorrowings);
            LoadChartData(_currentTimeRange, currentTrend);
        }
        private void HighlightTrendButton(Button selected)
        {
            if (activeTrendButton != null)
            {
                activeTrendButton.BackColor = Color.White;
                activeTrendButton.ForeColor = Color.Black;
            }

            selected.BackColor = Color.FromArgb(205, 133, 63);
            selected.ForeColor = Color.White;
            activeTrendButton = selected;
        }
        private void LoadChartData(string mode, string trend = "Borrowings")
        {
            if (chartReports == null)
                return;

            // Determine the time range
            string dateFormat = "%Y-%m-%d"; // daily by default
            string rangeCondition = "";

            if (mode == "today")
                rangeCondition = "DATE(BorrowDate) = DATE('now')";
            else if (mode == "week")
                rangeCondition = "DATE(BorrowDate) >= DATE('now','-6 day')";
            else if (mode == "month")
                rangeCondition = "strftime('%Y-%m', BorrowDate) = strftime('%Y-%m','now')";

            using (var con = Db.GetConnection())
            {
                con.Open();

                // Borrowings over time
                string borrowQuery = $@"
            SELECT DATE(BorrowDate) AS BorrowDay, COUNT(*) AS Count
            FROM Borrowings
            WHERE {rangeCondition}
            GROUP BY DATE(BorrowDate)
            ORDER BY DATE(BorrowDate);
        ";

                // Returns over time
                string returnQuery = $@"
            SELECT DATE(ReturnDate) AS ReturnDay, COUNT(*) AS Count
            FROM Borrowings
            WHERE ReturnDate IS NOT NULL 
              AND ReturnDate <> '' 
              AND (
                    {(mode == "today" ? "DATE(ReturnDate)=DATE('now')" :
                                mode == "week" ? "DATE(ReturnDate)>=DATE('now','-6 day')" :
                                "strftime('%Y-%m', ReturnDate)=strftime('%Y-%m','now')")}
                  )
            GROUP BY DATE(ReturnDate)
            ORDER BY DATE(ReturnDate);
        ";

                // Overdue trend
                string overdueQuery = $@"
            SELECT DATE(DueDate) AS DueDay, COUNT(*) AS Count
            FROM Borrowings
            WHERE ReturnDate IS NULL
              AND DATE(DueDate) <= DATE('now')
              AND (
                    {(mode == "today" ? "DATE(DueDate)=DATE('now')" :
                                mode == "week" ? "DATE(DueDate)>=DATE('now','-6 day')" :
                                "strftime('%Y-%m', DueDate)=strftime('%Y-%m','now')")}
                  )
            GROUP BY DATE(DueDate)
            ORDER BY DATE(DueDate);
        ";
                // --- Data storage ---
                var borrowData = new Dictionary<string, int>();
                var returnData = new Dictionary<string, int>();
                var overdueData = new Dictionary<string, int>();
                // Borrowings
                using (var cmd = new SQLiteCommand(borrowQuery, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        borrowData[reader["BorrowDay"].ToString()] = Convert.ToInt32(reader["Count"]);
                // Returns
                using (var cmd = new SQLiteCommand(returnQuery, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        returnData[reader["ReturnDay"].ToString()] = Convert.ToInt32(reader["Count"]);
                // Overdue
                using (var cmd = new SQLiteCommand(overdueQuery, con))
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        overdueData[reader["DueDay"].ToString()] = Convert.ToInt32(reader["Count"]);
                // --- Clear old series ---
                chartReports.Series.Clear();

                // Borrowing series
                Series borrowSeries = new Series("Borrowing Activity")
                {
                    ChartType = SeriesChartType.Spline,
                    BorderWidth = 3,
                    Color = Color.FromArgb(165, 105, 79), // Coffee Brown
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 8,
                    MarkerColor = Color.White,
                    MarkerBorderColor = Color.FromArgb(165, 105, 79)
                };
                foreach (var kv in borrowData)
                    borrowSeries.Points.AddXY(kv.Key, kv.Value);
                // Return series
                Series returnSeries = new Series("Returns")
                {
                    ChartType = SeriesChartType.Spline,
                    BorderWidth = 3,
                    Color = Color.FromArgb(205, 133, 63), // Peru
                    MarkerStyle = MarkerStyle.Circle,
                    MarkerSize = 8,
                    MarkerColor = Color.White,
                    MarkerBorderColor = Color.FromArgb(205, 133, 63)
                };
                foreach (var kv in returnData)
                    returnSeries.Points.AddXY(kv.Key, kv.Value);

                // Overdue series
                Series overdueSeries = new Series("Overdue")
                {
                    ChartType = SeriesChartType.Spline,
                    BorderWidth = 3,
                    Color = Color.FromArgb(128, 64, 0), // Dark Brown
                    MarkerStyle = MarkerStyle.Diamond,
                    MarkerSize = 8,
                    MarkerColor = Color.White,
                    MarkerBorderColor = Color.FromArgb(128, 64, 0)
                };
                foreach (var kv in overdueData)
                    overdueSeries.Points.AddXY(kv.Key, kv.Value);
                // Add series to chart
                chartReports.Series.Add(borrowSeries);
                chartReports.Series.Add(returnSeries);
                chartReports.Series.Add(overdueSeries);
                // Chart title (C# 7.3 style)
                string title = "📈 Borrowing Activity";
                if (mode == "today")
                    title = "📈 Borrowing Activity Today";
                else if (mode == "week")
                    title = "📈 Borrowing Activity This Week";
                else if (mode == "month")
                    title = "📈 Borrowing Activity This Month";
                chartReports.Titles.Clear();
                chartReports.Titles.Add(title);
                chartReports.Titles[0].Font = new Font("Segoe UI Emoji", 12, FontStyle.Bold);
                chartReports.Titles[0].ForeColor = Color.FromArgb(90, 50, 20);
            }
            chartReports.Invalidate();
        }
        public DashboardControl() : this("ADMIN") { }

        private void DashboardControl_Load(object sender, EventArgs e)
        {
            // 📊 Initialize the borrowing activity chart (upper section)
            SetupDashboardChart();

            // 🔄 Sync penalties first
            Penalties.SyncPenaltiesFromBorrowings();

            // 💰 Initialize the penalty summary card (bottom-right card)
            InitializePenaltySummaryCard();

            // 🎨 Apply consistent visual styles to the statistic cards
            StyleCard(pnlTotalBooks, Color.White);
            StyleCard(pnlBorrowedBooks, Color.White);
            StyleCard(pnlAvailableBooks, Color.White);
            StyleCard(pnlActiveMembers, Color.White);
            StyleCard(pnlOverdueBooks, Color.White);

            // 🟢 Add rounded corners for better UI
            ApplyCardStyle(pnlTotalBooks, 10);
            ApplyCardStyle(pnlBorrowedBooks, 10);
            ApplyCardStyle(pnlAvailableBooks, 10);
            ApplyCardStyle(pnlActiveMembers, 10);
            ApplyCardStyle(pnlOverdueBooks, 10);

            // 🖼 Apply image style for each PictureBox
            ApplyPictureBoxStyle(picTotalBooks, 2);
            ApplyPictureBoxStyle(picBorrowedBooks, 2);
            ApplyPictureBoxStyle(picAvailableBooks, 2);
            ApplyPictureBoxStyle(picActiveMembers, 2);
            ApplyPictureBoxStyle(picOverdueBooks, 2);

            // 🧩 Tidy up margins for cleaner layout
            foreach (Panel pnl in new[] { pnlTotalBooks, pnlBorrowedBooks, pnlAvailableBooks, pnlActiveMembers, pnlOverdueBooks })
            {
                pnl.Margin = new Padding(8);
            }

            //// ✅ Start auto-refresh for Penalty Summary (updates every 5 seconds)
            SetupPenaltyRefreshTimer();
        }

        private void InitializeRecentBorrowingsSection()
        {
            // 1️⃣ Create the panel
            pnlRecentBorrowings = new Panel
            {
                Width = 600,
                Height = 310,
                BackColor = Color.White,
                Location = new Point(226, 560)
            };
            // 4️⃣ Add it to the form (or parent container)
            this.Controls.Add(pnlRecentBorrowings);

            // Apply border radius
            int borderRadius = 10; // change to 2 or whatever you want
            pnlRecentBorrowings.Paint += (s, e) =>
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    Rectangle rect = new Rectangle(0, 0, pnlRecentBorrowings.Width, pnlRecentBorrowings.Height);
                    int radius = borderRadius * 2;

                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Top-left
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Top-right
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Bottom-right
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Bottom-left
                    path.CloseAllFigures();

                    pnlRecentBorrowings.Region = new Region(path);
                }
            };
            this.Controls.Add(pnlRecentBorrowings);
            // Title label
            lblRecentBorrowings = new Label
            {
                Text = "🕑 Recent Borrowings",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 50, 20),
                Height = 30,
                Width = pnlRecentBorrowings.Width - 20 // respect panel padding
            };
            // Add a top margin of 5% of panel height
            lblRecentBorrowings.Location = new Point(10, (int)(pnlRecentBorrowings.Height * 0.05));

            pnlRecentBorrowings.Controls.Add(lblRecentBorrowings);
            // DataGridView setup
            dgvRecentBorrowings = new DataGridView
            {
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                RowTemplate = { Height = 40 }
            };
            // Place DataGridView below the label with some spacing
            dgvRecentBorrowings.Location = new Point(0, lblRecentBorrowings.Bottom + 5);
            dgvRecentBorrowings.Size = new Size(pnlRecentBorrowings.Width, pnlRecentBorrowings.Height - lblRecentBorrowings.Bottom - 10);
            dgvRecentBorrowings.RowPostPaint += dgvRecentBorrowings_RowPostPaint;
            dgvRecentBorrowings.RowHeadersVisible = true;
            dgvRecentBorrowings.RowHeadersWidth = 50; // enough space for numbers
            // Attach DataBindingComplete only once
            dgvRecentBorrowings.DataBindingComplete += DgvRecentBorrowings_DataBindingComplete;
            dgvRecentBorrowings.SelectionChanged += DgvRecentBorrowings_SelectionChanged;
            // Styling
            dgvRecentBorrowings.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(205, 133, 63); // Peru highlight color
            dgvRecentBorrowings.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecentBorrowings.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvRecentBorrowings.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvRecentBorrowings.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            pnlRecentBorrowings.Controls.Add(dgvRecentBorrowings);
        }

        private Timer penaltySummaryTimer;

        private void InitializePenaltySummaryCard()
        {
            // Panel (card)
            Panel pnlPenaltySummary = new Panel
            {
                Width = 243,
                Height = 309,
                BackColor = Color.White,
                Location = new Point(1264, 561),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };

            ApplyCardStyles(pnlPenaltySummary, 15);

            // 💰 Title
            Label lblTitle = new Label
            {
                Text = "💰 Penalty Summary",
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 50, 20),
                AutoSize = true,
                Location = new Point(15, 10)
            };
            pnlPenaltySummary.Controls.Add(lblTitle);

            // Subtext
            Label lblSub = new Label
            {
                Text = "Total Penalties Collected (This month)",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(18, 45)
            };
            pnlPenaltySummary.Controls.Add(lblSub);

            // 💸 Total Collected
            Label lblCollected = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 28F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                AutoSize = true,
                Location = new Point(15, 65),
                Name = "lblCollected"
            };
            pnlPenaltySummary.Controls.Add(lblCollected);

            // Divider
            Panel line = new Panel
            {
                BackColor = Color.LightGray,
                Size = new Size(320, 1),
                Location = new Point(10, 120)
            };
            pnlPenaltySummary.Controls.Add(line);

            // Period
            Label lblPeriodText = new Label
            {
                Text = "Period:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(20, 135)
            };
            pnlPenaltySummary.Controls.Add(lblPeriodText);

            Label lblPeriod = new Label
            {
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(140, 135),
                Name = "lblPeriod"
            };
            pnlPenaltySummary.Controls.Add(lblPeriod);

            // 💀 Unpaid Fines
            Label lblUnpaidText = new Label
            {
                Text = "Unpaid Fines:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(20, 155)
            };
            pnlPenaltySummary.Controls.Add(lblUnpaidText);

            Label lblUnpaid = new Label
            {
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.OrangeRed,
                AutoSize = true,
                Location = new Point(140, 155),
                Name = "lblUnpaid"
            };
            pnlPenaltySummary.Controls.Add(lblUnpaid);

            // Add to dashboard
            this.Controls.Add(pnlPenaltySummary);

            // Load data initially
            LoadPenaltySummaryFromBorrowings(lblCollected, lblUnpaid, lblPeriod);

            // ✅ Set up timer to refresh every 10 seconds
            penaltySummaryTimer = new Timer();
            penaltySummaryTimer.Interval = 10000; // 10,000 ms = 10 seconds
            penaltySummaryTimer.Tick += (s, e) =>
            {
                LoadPenaltySummaryFromBorrowings(lblCollected, lblUnpaid, lblPeriod);
            };
            penaltySummaryTimer.Start();
        }

        private void SetupPenaltyRefreshTimer()
        {
            penaltyTimer = new Timer { Interval = 5000 };
            penaltyTimer.Tick += (s, e) =>
            {
                // Sync first
                Penalties.SyncPenaltiesFromBorrowings();

                // Then refresh UI
                RefreshPenaltySummary();
            };
            penaltyTimer.Start();
        }



        // ✅ Loads and updates the Penalty Summary Card dynamically
        private void LoadPenaltySummaryFromBorrowings(Label lblCollected, Label lblUnpaid, Label lblPeriod)
        {
            try
            {
                using (SQLiteConnection con = Db.GetConnection())
                {
                    con.Open();

                    DateTime now = DateTime.Now;
                    string startOfMonth = new DateTime(now.Year, now.Month, 1).ToString("yyyy-MM-dd");
                    string endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).ToString("yyyy-MM-dd");

                    // Total penalties collected this month
                    string sqlCollected = @"
                            SELECT IFNULL(SUM(Amount), 0)
                            FROM Penalties
                            WHERE Status = 'Paid'
                              AND strftime('%Y-%m', PaidDate) = strftime('%Y-%m', 'now')
                        ";
                    lblCollected.Text = "₱" + new SQLiteCommand(sqlCollected, con).ExecuteScalar().ToString();

                    using (var cmd = new SQLiteCommand(sqlCollected, con))
                    {
                        cmd.Parameters.AddWithValue("@start", startOfMonth);
                        cmd.Parameters.AddWithValue("@end", endOfMonth);
                        decimal totalCollected = Convert.ToDecimal(cmd.ExecuteScalar());
                        lblCollected.Text = $"₱{totalCollected:N2}";
                    }

                    // Total unpaid fines
                    string sqlUnpaid = @"
                            SELECT IFNULL(SUM(Amount), 0)
                            FROM Penalties
                            WHERE Status = 'Unpaid'
                        ";
                    lblUnpaid.Text = "₱" + new SQLiteCommand(sqlUnpaid, con).ExecuteScalar().ToString();

                    // Period
                    lblPeriod.Text = DateTime.Now.ToString("MMMM yyyy");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading penalty summary: " + ex.Message);
            }
        }



        public void RefreshPenaltySummary()
        {
            // Find the panel by name or reference
            var pnlPenaltySummary = this.Controls
                .OfType<Panel>()
                .FirstOrDefault(p => p.Controls.OfType<Label>().Any(l => l.Name == "lblCollected"));

            if (pnlPenaltySummary == null) return;

            // Find the dynamically created labels
            var lblCollected = pnlPenaltySummary.Controls["lblCollected"] as Label;
            var lblUnpaid = pnlPenaltySummary.Controls["lblUnpaid"] as Label;

            if (lblCollected == null || lblUnpaid == null) return;

            // ✅ Use the new method that calculates totals for THIS MONTH
            var (totalPaidThisMonth, totalUnpaid) = Penalties.GetPenaltySummary();

            // Update labels
            lblCollected.Text = $"₱{totalPaidThisMonth:N2}";
            lblUnpaid.Text = $"₱{totalUnpaid:N2}";

            // Optional: update period dynamically
            var lblPeriod = pnlPenaltySummary.Controls["lblPeriod"] as Label;
            if (lblPeriod != null)
            {
                lblPeriod.Text = DateTime.Now.ToString("MMMM yyyy");
            }
        }



        private void LoadMostBorrowedBooks()
        {
            panelMostBorrowed.Controls.Clear();

            Label title = new Label
            {
                Text = "🏆 Most Borrowed Books",
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 50, 20),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
              
            };
            panelMostBorrowed.Controls.Add(title);

            // Apply border radius
            int borderRadius = 10; // change to 2 or whatever you want
            panelMostBorrowed.Paint += (s, e) =>
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    Rectangle rect = new Rectangle(0, 0, panelMostBorrowed.Width, panelMostBorrowed.Height);
                    int radius = borderRadius * 2;

                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90); // Top-left
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90); // Top-right
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90); // Bottom-right
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90); // Bottom-left
                    path.CloseAllFigures();

                    panelMostBorrowed.Region = new Region(path);
                }
            };

            var topBooks = GetMostBorrowedBooks();

            if (topBooks.Count == 0)
            {
                Label lblNone = new Label
                {
                    Text = "No borrowed books yet.",
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panelMostBorrowed.Controls.Add(lblNone);
                return;
            }

            int maxBorrows = topBooks[0].Borrows; // highest borrow count
            int y = 60;

            foreach (var (Title, Borrows) in topBooks)
            {
                Label lblBook = new Label
                {
                    Text = $"{Title} — {Borrows} borrows",
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    Location = new Point(20, y),
                    AutoSize = true
                };

                ProgressBar progress = new ProgressBar
                {
                    Size = new Size(250, 10),
                    Location = new Point(20, y + 25),
                    Maximum = maxBorrows,
                    Value = Borrows,
                };

                panelMostBorrowed.Controls.Add(lblBook);
                panelMostBorrowed.Controls.Add(progress);
                y += 45;
            }
        }

        private List<(string Title, int Borrows)> GetMostBorrowedBooks()
        {
            var list = new List<(string, int)>();

            using (var con = Db.GetConnection())
            {
                con.Open();
                string query = @"
                    SELECT b.Title, COUNT(*) AS BorrowCount
                    FROM Borrowings br
                    INNER JOIN Books b ON br.BookId = b.BookId
                    GROUP BY b.Title
                    ORDER BY BorrowCount DESC
                    LIMIT 5;
                ";

                using (var cmd = new SQLiteCommand(query, con))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string title = reader["Title"].ToString();
                        int count = Convert.ToInt32(reader["BorrowCount"]);
                        list.Add((title, count));
                    }
                }
            }

            return list;
        }
    



        // optional for rounded corners / shadows
        private void ApplyCardStyles(Panel panel, int radius)
        {
            panel.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panel.Width, panel.Height, radius, radius));
        }
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
           int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
           int nWidthEllipse, int nHeightEllipse);
    

        private void DgvRecentBorrowings_SelectionChanged(object sender, EventArgs e)
        {
            dgvRecentBorrowings.ClearSelection();
            dgvRecentBorrowings.CurrentCell = null;
        }


        private void DgvRecentBorrowings_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvRecentBorrowings.ClearSelection();  // remove pre-selected row
            dgvRecentBorrowings.CurrentCell = null; // no active cell
        }

        private void dgvRecentBorrowings_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // draw row number in row header
            using (SolidBrush b = new SolidBrush(dgvRecentBorrowings.RowHeadersDefaultCellStyle.ForeColor))
            {
                string rowNumber = (e.RowIndex + 1).ToString();
                e.Graphics.DrawString(
                    rowNumber,
                    dgvRecentBorrowings.DefaultCellStyle.Font,
                    b,
                    e.RowBounds.Location.X + 15,
                    e.RowBounds.Location.Y + 10
                );
            }
        }
        private void LoadRecentBorrowings()
        {
            try
            {
                using (var connection = new SQLiteConnection(Db.ConnectionString))
                {
                    connection.Open();

                    string query = @"
                SELECT 
                    (m.FirstName || ' ' || m.LastName) AS MemberName,
                    bk.title AS BookTitle,
                    b.borrowDate AS BorrowDate,
                    b.dueDate AS DueDate,
                    b.status AS Status
                FROM borrowings b
                JOIN members m ON b.MemberId = m.MemberId
                JOIN books bk ON b.BookId = bk.BookId
                ORDER BY b.BorrowDate DESC
                LIMIT 10;
            ";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);

                        dgvRecentBorrowings.DataSource = table;
                        dgvRecentBorrowings.ClearSelection();
                        dgvRecentBorrowings.CurrentCell = null;

                        // Rename columns
                        dgvRecentBorrowings.Columns["MemberName"].HeaderText = "Member";
                        dgvRecentBorrowings.Columns["BookTitle"].HeaderText = "Book";
                        dgvRecentBorrowings.Columns["BorrowDate"].HeaderText = "Borrowed On";
                        dgvRecentBorrowings.Columns["DueDate"].HeaderText = "Due Date";
                        dgvRecentBorrowings.Columns["Status"].HeaderText = "Status";

                        // Format dates
                        dgvRecentBorrowings.Columns["BorrowDate"].DefaultCellStyle.Format = "MMM dd, yyyy";
                        dgvRecentBorrowings.Columns["DueDate"].DefaultCellStyle.Format = "MMM dd, yyyy";

                        // Row color by status
                        foreach (DataGridViewRow row in dgvRecentBorrowings.Rows)
                        {
                            string status = row.Cells["Status"].Value?.ToString();

                            if (status == "Borrowed")
                                row.DefaultCellStyle.ForeColor = Color.FromArgb(30, 144, 255); // DodgerBlue
                            else if (status == "Returned")
                                row.DefaultCellStyle.ForeColor = Color.FromArgb(34, 139, 34); // ForestGreen
                            else if (status == "Overdue")
                                row.DefaultCellStyle.ForeColor = Color.FromArgb(205, 92, 92); // IndianRed
                        }

                        // Auto-size columns to fit content
                        dgvRecentBorrowings.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load recent borrowings: " + ex.Message);
            }
        }
        private void StyleCard(Panel panel, Color bgColor)
        {
            panel.BackColor = bgColor;
            //panel.Cursor = Cursors.Hand;

            // Hover effect
            panel.MouseEnter += (s, e) => panel.BackColor = ControlPaint.Light(bgColor, 0.2f);
            panel.MouseLeave += (s, e) => panel.BackColor = bgColor;

         
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
        private void lblTotalBooks_Click(object sender, EventArgs e) { }
        private void lblAvailableBooks_Click(object sender, EventArgs e) { }
        private void lblActiveMembers_Click(object sender, EventArgs e) { }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _greetingTimer?.Stop();
            _greetingTimer?.Dispose();
            _statsTimer?.Stop();
            _statsTimer?.Dispose();
            base.OnHandleDestroyed(e);
        }
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
        private void panel1_Paint(object sender, PaintEventArgs e) { }
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

        private void panelMostBorrowed_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pnlPenaltySummary_Paint(object sender, PaintEventArgs e)
        {

        }

        private void picOverdueBooks_Click(object sender, EventArgs e){ }
        private void lblGreeting_Click(object sender, EventArgs e){ }
    }
}
