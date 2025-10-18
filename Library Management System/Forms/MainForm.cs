using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Library_Management_System.User_Control;
using LibraryManagementSystem;
using LibraryManagementSystem.Data;
using System.Runtime.InteropServices;

//cpanel password = GabrielCpanelAccount1
//Hostinger account = GabrielHosting1

namespace Library_Management_System
{
    public partial class MainForm : Form
    {
        // Add this property here
        public bool IsAdmin { get; set; } = false;
        public string Username { get; set; } = "ADMIN";


        // Inside MainForm.Designer.cs
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private Panel bottomSpacer;


        private bool isCollapsed = false; // Add this near your other fields (top of the class)
        private bool isDarkMode = false; // start with dark mode
                                        // store original texts so we can restore them exactly
        private Dictionary<Button, string> originalButtonTexts = new Dictionary<Button, string>();

        private Timer sidebarTimer = new Timer();
        private int sidebarMinWidth = 60;
        private int sidebarMaxWidth = 200;
        private int sidebarStep = 10;  // change speed — larger = faster
                                       // Add these near your other private fields
        private Button btnAccountSettings;
        private HoverPanelWithArrow settingsHoverPanel;

        private Panel sidebarIndicator;
        private Button activeButton;

        private Timer moveTimer;
        private int targetTop;




        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
        );




        public MainForm()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            // Setup timer
            sidebarTimer.Interval = 15; // ms per step
            sidebarTimer.Tick += sidebarTimer_Tick;

            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            button6.Cursor = Cursors.Hand;


            // MainForm
            this.Controls.Add(this.pictureBoxLogo);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);


        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            if (isCollapsed)
            {
                // Expand
                if (panel1.Width < sidebarMaxWidth)
                {
                    panel1.Width += sidebarStep;
                    if (panel1.Width >= sidebarMaxWidth)
                    {
                        sidebarTimer.Stop();
                        panel1.Width = sidebarMaxWidth;
                        isCollapsed = false;
                        // restore texts, alignments, and the collapse icon
                        ApplyExpandedState();
                    }
                }
            }
            else
            {
                // Collapse
                if (panel1.Width > sidebarMinWidth)
                {
                    panel1.Width -= sidebarStep;
                    if (panel1.Width <= sidebarMinWidth)
                    {
                        sidebarTimer.Stop();
                        panel1.Width = sidebarMinWidth;
                        isCollapsed = true;
                        // hide texts, center icons, change to expand icon
                        ApplyCollapsedState();
                    }
                }
            }
        }

        private void ApplyCollapsedState()
        {
            foreach (Control ctrl in panel1.Controls)
            {
                if (ctrl is Button btn)
                {
                    // Hide text for all except the collapse toggle (button8)
                    if (btn != button8)
                    {
                        btn.Text = "";
                        btn.ImageAlign = ContentAlignment.MiddleCenter;
                        btn.TextAlign = ContentAlignment.MiddleCenter;
                        btn.Padding = new Padding(0);
                    }
                }
            }

            // Collapse button (▶)
            button8.Text = "";
            button8.Image = Properties.Resources.collapse_icon; // right arrow
            button8.ImageAlign = ContentAlignment.MiddleCenter;
            button8.Padding = new Padding(0);
        }



        private void ApplyExpandedState()
        {
            // ✅ Restore Dashboard text here!
            btnDashboard.Text = "  Dashboard";

            // Restore button texts
            button1.Text = "  Manage Books";
            button2.Text = "  Manage Members";
            button3.Text = "  Borrow Books";
            button4.Text = "  Return Books";
            button5.Text = "  Reports";
            button6.Text = "  Settings";
            button7.Text = isDarkMode ? "  Light UI" : "  Dark UI";
            button8.Text = "  Collapse";

            foreach (Control ctrl in panel1.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.ImageAlign = ContentAlignment.MiddleLeft;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(10, 0, 0, 0);
                }
            }

            // Collapse icon (◀)
            button8.Image = Properties.Resources.Collapse; // left arrow
            button8.ImageAlign = ContentAlignment.MiddleLeft;
        }







        private void button1_Click(object sender, EventArgs e)
        {
            SetActiveButton(button1);
            // New way (loads UserControl into panelContainer):
            LoadControl(new ManageBooksControl());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetActiveButton(button2);
            LoadControl(new ManageMembersControl());
        }
        private void MoveIndicatorSmooth(object sender, EventArgs e)
        {
            int currentTop = sidebarIndicator.Top;
            int diff = targetTop - currentTop;

            if (Math.Abs(diff) < 1)
            {
                sidebarIndicator.Top = targetTop;
                moveTimer.Stop();
                return;
            }

            // --- Smooth easing ---
            // The factor controls speed and smoothness
            // 0.15 = smoother, slower ease
            double easingFactor = 0.20;
            int newTop = (int)(currentTop + diff * easingFactor);

            sidebarIndicator.Top = newTop;
        }




        private void MainForm_Load(object sender, EventArgs e)
        {
            // === Load Dashboard by default ===
            LoadControl(new DashboardControl(Username));

            moveTimer = new Timer { Interval = 15 };
            moveTimer.Tick += MoveIndicatorSmooth;


            // === Create sidebar selection indicator ===
            sidebarIndicator = new Panel();
            sidebarIndicator.Size = new Size(4, 40); // thin vertical bar
            sidebarIndicator.BackColor = Color.FromArgb(205, 173, 132); // light brown
            sidebarIndicator.Visible = false;
            panel1.Controls.Add(sidebarIndicator);




            // === Ensure pictureBoxLogo is inside panel1 ===
            if (!panel1.Controls.Contains(pictureBoxLogo))
            {
                // --- Create a top spacer (5% height) ---
                Panel topSpacer = new Panel();
                topSpacer.Dock = DockStyle.Top;
                topSpacer.Height = (int)(panel1.Height * 0.02); // 5% of sidebar height
                topSpacer.BackColor = Color.Transparent; // invisible spacer
                panel1.Controls.Add(topSpacer);

                pictureBoxLogo.Dock = DockStyle.Top;
                pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxLogo.Height = 0; // adjust to fit your logo
                panel1.Controls.Add(pictureBoxLogo);
                // Keep logo above spacer visually
                panel1.Controls.SetChildIndex(pictureBoxLogo, 0);

                // Resize dynamically when window resizes
                this.Resize += (s, ev) =>
                {
                    topSpacer.Height = (int)(panel1.Height * 0.02);
                };
            }

            // === Create floating hover panel ===
            // === Create floating hover panel with arrow ===
            settingsHoverPanel = new HoverPanelWithArrow();
            settingsHoverPanel.BackColor = Color.FromArgb(30, 30, 30);
            settingsHoverPanel.Visible = false;
            settingsHoverPanel.Size = new Size(180, 80);
            settingsHoverPanel.BorderStyle = BorderStyle.None;
            settingsHoverPanel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, settingsHoverPanel.Width, settingsHoverPanel.Height, 10, 10));
            this.Controls.Add(settingsHoverPanel);
            settingsHoverPanel.BringToFront();


            // === Create dropdown buttons ===
            btnAccountSettings = new Button();
            btnLogout = new Button();

            btnAccountSettings.Text = "            All Settings";
            btnLogout.Text = "            Log Out";

            // Optional: add icons (if available in your project resources)
            btnAccountSettings.Image = Properties.Resources.Setting;
            btnLogout.Image = Properties.Resources.Logout1;

            foreach (var btn in new[] { btnLogout, btnAccountSettings })
            {
                btn.Dock = DockStyle.Top;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
                btn.ForeColor = Color.Black;
                btn.BackColor = Color.LightGray;
                btn.Height = 40;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(10, 0, 0, 0);
                btn.ImageAlign = ContentAlignment.MiddleLeft;
                btn.FlatAppearance.MouseOverBackColor = Color.Gray;
                btn.Cursor = Cursors.Hand;
                settingsHoverPanel.Controls.Add(btn);
            }

            // === Hover behavior ===
            // === Hover behavior (fixed timing & smooth disappearance) ===
            button6.MouseEnter += (s, ev) =>
            {
                // Position hover panel beside Settings button
                int arrow = settingsHoverPanel.ArrowSize;
                Point buttonLocation = button6.PointToScreen(Point.Empty);
                Point panelLocation = this.PointToClient(new Point(
                    buttonLocation.X + button6.Width - arrow,  // shift slightly left
                    buttonLocation.Y - 10
                ));
                settingsHoverPanel.Location = panelLocation;




                settingsHoverPanel.Location = panelLocation;
                settingsHoverPanel.Visible = true;
                settingsHoverPanel.BringToFront();
            };

            // Track whether mouse is over either the button or the hover panel
            bool isHoveringSettingsArea = false;
            Timer hoverCheckTimer = new Timer { Interval = 100 }; // check every 0.1 sec
            hoverCheckTimer.Tick += (s, ev) =>
            {
                // If mouse is outside both button6 and settingsHoverPanel, hide immediately
                if (!button6.Bounds.Contains(PointToClient(Cursor.Position)) &&
                    !settingsHoverPanel.Bounds.Contains(PointToClient(Cursor.Position)))
                {
                    settingsHoverPanel.Visible = false;
                    hoverCheckTimer.Stop();
                }
            };

            // When mouse leaves the settings button
            button6.MouseLeave += (s, ev) =>
            {
                hoverCheckTimer.Start(); // start checking frequently
            };

            // When mouse enters hover panel, keep it visible
            settingsHoverPanel.MouseEnter += (s, ev) =>
            {
                hoverCheckTimer.Stop();
                settingsHoverPanel.Visible = true;
            };

            // When mouse leaves hover panel, hide quickly
            settingsHoverPanel.MouseLeave += (s, ev) =>
            {
                hoverCheckTimer.Start();
            };



            // === Click actions ===
            btnAccountSettings.Click += (s, ev) =>
            {
                MessageBox.Show("Opening all settings...");
            };

            btnLogout.Click += (s, ev) =>
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to log out?",
                    "Confirm Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (result == DialogResult.Yes)
                {
                    this.Close();
                }
            };

            // === Fix button order manually (bottom to top) ===
            if (panel1.Controls.Contains(button8)) panel1.Controls.SetChildIndex(button8, 0);
            if (panel1.Controls.Contains(button7)) panel1.Controls.SetChildIndex(button7, 1);
            if (panel1.Controls.Contains(button6)) panel1.Controls.SetChildIndex(button6, 2);
            if (panel1.Controls.Contains(button5)) panel1.Controls.SetChildIndex(button5, 3);
            if (panel1.Controls.Contains(button4)) panel1.Controls.SetChildIndex(button4, 4);
            if (panel1.Controls.Contains(button3)) panel1.Controls.SetChildIndex(button3, 5);
            if (panel1.Controls.Contains(button2)) panel1.Controls.SetChildIndex(button2, 6);
            if (panel1.Controls.Contains(button1)) panel1.Controls.SetChildIndex(button1, 7);
            if (panel1.Controls.Contains(btnDashboard)) panel1.Controls.SetChildIndex(btnDashboard, 8);
            if (panel1.Controls.Contains(pictureBoxLogo)) panel1.Controls.SetChildIndex(pictureBoxLogo, 9);

            // === Add bottom spacer ===
            bottomSpacer = new Panel();
            bottomSpacer.Dock = DockStyle.Bottom;
            bottomSpacer.Height = (int)(this.ClientSize.Height * 0.05);
            bottomSpacer.BackColor = Color.White;
            this.Controls.Add(bottomSpacer);

            this.Resize += (s, ev) =>
            {
                bottomSpacer.Height = (int)(this.ClientSize.Height * 0.05);
            };
        }

        public void OpenManageBooksFromDashboard()
        {
            SetActiveButton(button1); // button1 = Manage Books
            LoadControl(new ManageBooksControl());
        }

        public void OpenManageMembersFromDashboard()
        {
            SetActiveButton(button2); // button2 = Manage Members
            LoadControl(new ManageMembersControl());
        }


        public void SetActiveButton(Button clickedButton)
        {
            // Reset previous active button
            if (activeButton != null)
            {
                activeButton.ForeColor = Color.Black; // default text color
                activeButton.BackColor = Color.Transparent;
            }

            // Set new active button
            activeButton = clickedButton;
            activeButton.ForeColor = Color.FromArgb(205, 173, 132); // light brown
            activeButton.BackColor = Color.White; // optional very light beige highlight

            // Move and show the indicator beside it
            sidebarIndicator.Height = activeButton.Height;
            sidebarIndicator.Left = panel1.Width - sidebarIndicator.Width; // right edge
            sidebarIndicator.Visible = true;
            sidebarIndicator.BringToFront();

            targetTop = activeButton.Top;
            moveTimer.Start();
        }


        // Custom panel with arrow pointer
        public class HoverPanelWithArrow : Panel
        {
            public int ArrowSize { get; set; } = 10;
            public int ArrowOffsetY { get; set; } = 30;
            public Color PanelColor { get; set; } = Color.FromArgb(245, 245, 245); // solid light gray
            public Color ArrowColor { get; set; } = Color.FromArgb(245, 245, 245); // same as panel color
            public Color BorderColor { get; set; } = Color.Silver;

            public HoverPanelWithArrow()
            {
                this.DoubleBuffered = true;
                this.BackColor = Color.Transparent; // form background shows through cleanly
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Build single combined path (arrow + panel)
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

                path.AddPolygon(new Point[]
                {
            new Point(0, ArrowOffsetY),                // arrow tip
            new Point(ArrowSize, ArrowOffsetY - 7),    // arrow top
            new Point(ArrowSize, 0),                   // panel top-left
            new Point(Width, 0),                       // top-right
            new Point(Width, Height),                  // bottom-right
            new Point(ArrowSize, Height),              // bottom-left
            new Point(ArrowSize, ArrowOffsetY + 7)     // arrow bottom
                });

                // Fill the combined shape
                using (SolidBrush brush = new SolidBrush(PanelColor))
                    e.Graphics.FillPath(brush, path);

                // Optional subtle border
                using (Pen borderPen = new Pen(BorderColor, 1))
                    e.Graphics.DrawPath(borderPen, path);
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                this.Padding = new Padding(this.ArrowSize, 0, 0, 0);
                this.Invalidate();
            }
        }






        private void button6_MouseEnter(object sender, EventArgs e)
        {
            // Show dropdown items
            btnAccountSettings.Visible = true;
            btnLogout.Visible = true;
        }

        private void button6_MouseLeave(object sender, EventArgs e)
        {
            // Delay hiding slightly to avoid flicker when moving between buttons
            Timer hideTimer = new Timer();
            hideTimer.Interval = 1; // milliseconds
            hideTimer.Tick += (s, ev) =>
            {
                if (!btnAccountSettings.Bounds.Contains(PointToClient(Cursor.Position)) &&
                    !btnLogout.Bounds.Contains(PointToClient(Cursor.Position)) &&
                    !button6.Bounds.Contains(PointToClient(Cursor.Position)))
                {
                    btnAccountSettings.Visible = false;
                    btnLogout.Visible = false;
                }
                hideTimer.Stop();
            };
            hideTimer.Start();
        }




        private void btnDashboard_Click(object sender, EventArgs e)
        {
            SetActiveButton(btnDashboard);
            LoadControl(new DashboardControl());
        }

        //private void btnLogout_Click(object sender, EventArgs e)
        //{
        //    //// Ask user for confirmation
        //    //DialogResult result = MessageBox.Show(
        //    //    "Are you sure you want to logout?",
        //    //    "Confirm Logout",
        //    //    MessageBoxButtons.YesNo,
        //    //    MessageBoxIcon.Question
        //    //);


        //    //if (result == DialogResult.Yes)
        //    //{
        //    //    // Only logout if user clicks Yes
        //    //    MessageBox.Show("Logout successful!", "Logout", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    //    this.Close();
        //    //}
        //    //// If user clicks No, nothing happens
        //}


        private void panelContainer_Paint(object sender, PaintEventArgs e)
        {

        }
        public void LoadControl(UserControl control)
        {
            panelContainer.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelContainer.Controls.Add(control);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            LoadControl(new DashboardControl());
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            //// Keep smooth graphics
            //e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //int shadowWidth = 8; // width of shadow (adjust if needed)
            //Rectangle shadowRect = new Rectangle(panel1.Width - shadowWidth, 0, shadowWidth, panel1.Height);

            //// Create a gradient shadow from right to left
            //using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
            //    shadowRect,
            //    Color.FromArgb(100, Color.Black), // semi-transparent black
            //    Color.Transparent,                // fade out to transparent
            //    0f                                // horizontal gradient
            //))
            //{
            //    e.Graphics.FillRectangle(brush, shadowRect);
            //}
        }


        private void button3_Click(object sender, EventArgs e)
        {
            SetActiveButton(button3);
            LoadControl(new BorrowBooksControl());
        }

        private void button4_Click(object sender, EventArgs e)
        {

            SetActiveButton(button4);
            // Clear existing controls in the panelContainer
            panelContainer.Controls.Clear();

            // Create instance of ReturnBooksControl
            ReturnBooksControl returnBooks = new ReturnBooksControl();
            returnBooks.Dock = DockStyle.Fill;

            // Add it to the container
            panelContainer.Controls.Add(returnBooks);
        }


        private void button5_Click(object sender, EventArgs e)
        {
            SetActiveButton(button5);

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Start the timer to animate
            sidebarTimer.Start();
        }





        private void button7_Click(object sender, EventArgs e)
        {
            if (isDarkMode)
            {
                // Switch to Light Mode
                ApplyTheme(this, Color.White, Color.Black,false);
                button7.Text = "    Dark UI";
                isDarkMode = false;
            }
            else
            {
                // Switch to Dark Mode
                ApplyTheme(this, Color.FromArgb(30, 30, 30), Color.White, true);
                button7.Text = "    Light UI";
                isDarkMode = true;
            }
        }

        private void ApplyTheme(Control parent, Color backColor, Color foreColor, bool dark)
        {
            parent.BackColor = backColor;
            parent.ForeColor = foreColor;

            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Panel)
                {
                    ctrl.BackColor = backColor;
                    ApplyTheme(ctrl, backColor, foreColor, dark);
                }
                else if (ctrl is Button btn)
                {
                    btn.BackColor = backColor;
                    btn.ForeColor = foreColor;
                    btn.FlatAppearance.BorderColor = dark ? Color.Gray : Color.DarkGray;
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = foreColor;
                }
                else if (ctrl is PictureBox pic)
                {
                    pic.BackColor = backColor;
                }
                else
                {
                    ctrl.BackColor = backColor;
                    ctrl.ForeColor = foreColor;
                }
            }
        }






        private void button6_Click(object sender, EventArgs e)
        {


        }

        private void button6_Click_1(object sender, EventArgs e)
        {

        }




        //private void button1_Click(object sender, EventArgs e)
        //{
        //    using (var con = Db.GetConnection())
        //    {
        //        con.Open();
        //        MessageBox.Show("Database connection successful!");
        //    }
        //}
    }
}
