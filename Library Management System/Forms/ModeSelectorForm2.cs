using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Library_Management_System.Forms
{
    public partial class ModeSelectorForm2 : Form
    {
        private Panel pnlAdminLogin;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Timer rollDownTimer;
        private int targetHeight = 290;
        private int animationStep = 0;

        // Win32 AnimateWindow
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool AnimateWindow(IntPtr hWnd, int dwTime, int dwFlags);

        private const int AW_VER_POSITIVE = 0x0004;
        private const int AW_ACTIVATE = 0x20000;
        private const int AW_BLEND = 0x80000;
        private const int AW_SLIDE = 0x40000;

        public ModeSelectorForm2()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += ModeSelectorForm2_KeyDown;
            this.StartPosition = FormStartPosition.CenterParent; // Center modal

        }

        private void ModeSelectorForm2_Load(object sender, EventArgs e)
        {
            // Apply rounded button to Admin button
            SetButtonRounded(btnAdmin, 20);

            // Create login panel AFTER designer loads btnAdmin
            CreateAdminLoginPanel();
        }

        // Handle Enter key for login
        private void ModeSelectorForm2_KeyDown(object sender, KeyEventArgs e)
        {
            if (pnlAdminLogin.Visible && e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(btnLogin, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private void SetButtonRounded(Button btn, int radius)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TabStop = false;

            btn.Resize += (s, e) => ApplyRoundedRegion(btn, radius);
            btn.Paint += Btn_Paint;

            ApplyRoundedRegion(btn, radius);
        }

        private void ApplyRoundedRegion(Button btn, int radius)
        {
            Rectangle rect = btn.ClientRectangle;
            int diameter = radius * 2;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                btn.Region = new Region(path);
            }
        }

        private void Btn_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            int radius = 20;
            float penWidth = 1.4f;
            Color borderColor = Color.FromArgb(120, 56, 19, 19);

            RectangleF rect = new RectangleF(
                penWidth / 2f,
                penWidth / 2f,
                btn.Width - penWidth - 1f,
                btn.Height - penWidth - 1f
            );

            float diameter = radius * 2;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                using (Pen pen = new Pen(borderColor, penWidth))
                    e.Graphics.DrawPath(pen, path);
            }
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            btnAdmin.Enabled = false;
            //btnStudent.Enabled = false;



            // Apply a smooth "roll down" animation to the whole form first
            AnimateWindow(this.Handle, 400, AW_VER_POSITIVE | AW_SLIDE | AW_ACTIVATE | AW_BLEND);

            // Then roll the admin panel down (simulating form filling)
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            pnlAdminLogin.Visible = true;
            pnlAdminLogin.Height = 0;

            // Center the panel
            pnlAdminLogin.Location = new Point(
                (this.ClientSize.Width - pnlAdminLogin.Width) / 2,
                (this.ClientSize.Height - targetHeight) / 2
            );

            animationStep = 0;
            rollDownTimer = new Timer
            {
                Interval = 12 // smoother speed
            };
            rollDownTimer.Tick += RollDownEffect;
            rollDownTimer.Start();
        }

        // 🎞️ Roll-down + smooth easing effect for the panel
        private void RollDownEffect(object sender, EventArgs e)
        {
            animationStep++;

            double progress = animationStep / 25.0; // total duration
            double eased = Math.Sin(progress * Math.PI / 2); // smooth easing curve

            pnlAdminLogin.Height = (int)(targetHeight * eased);
            pnlAdminLogin.Location = new Point(
                (this.ClientSize.Width - pnlAdminLogin.Width) / 2,
                (this.ClientSize.Height - pnlAdminLogin.Height) / 2
            );

            if (animationStep >= 25)
            {
                pnlAdminLogin.Height = targetHeight;
                rollDownTimer.Stop();
                rollDownTimer.Dispose();
            }
        }

        // 🧱 Create the admin login panel (updated to look smoother + better centered)
        private void CreateAdminLoginPanel()
        {
            pnlAdminLogin = new Panel
            {
                Visible = false,
                Height = 0,
                Width = 360
            };
            pnlAdminLogin.Location = new Point(
                (this.ClientSize.Width - pnlAdminLogin.Width) / 2,
                (this.ClientSize.Height - pnlAdminLogin.Height) / 2
            );
            this.Controls.Add(pnlAdminLogin);

            Label lblTitle = new Label
            {
                Text = "Admin Login",
                Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold),
                ForeColor = ColorTranslator.FromHtml("#381313"),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            pnlAdminLogin.Controls.Add(lblTitle);

            Label lblUser = new Label
            {
                Text = "Username",
                Font = new Font("Microsoft Sans Serif", 11),
                ForeColor = SystemColors.ControlText,
                Location = new Point(45, 85),
                AutoSize = true
            };
            pnlAdminLogin.Controls.Add(lblUser);

            // Panel wrapper for Username TextBox (custom border)
            Panel pnlUsernameBorder = new Panel
            {
                Width = 270,
                Height = 34, // a bit taller to match styling
                Location = new Point(45, 110),
                BackColor = Color.Transparent
            };
            pnlUsernameBorder.Paint += (s, e) =>
            {
                int radius = 6;
                int penWidth = 1;
                Color borderColor = ColorTranslator.FromHtml("#E5E7EB");

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, pnlUsernameBorder.Width - 1, pnlUsernameBorder.Height - 1);

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (Pen pen = new Pen(borderColor, penWidth))
                    {
                        pen.LineJoin = LineJoin.Round;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };


            txtUsername = new TextBox
            {
                Width = 270,
                Location = new Point(45, 110),
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };

            pnlAdminLogin.Controls.Add(txtUsername);
            pnlAdminLogin.Controls.Add(pnlUsernameBorder);

            Label lblPass = new Label
            {
                Text = "Password",
                Font = new Font("Microsoft Sans Serif", 11),
                ForeColor = SystemColors.ControlText,
                Location = new Point(45, 150),
                AutoSize = true
            };
            pnlAdminLogin.Controls.Add(lblPass);

            // Panel wrapper for Password TextBox (custom border)
            Panel pnlPasswordBorder = new Panel
            {
                Width = 270,
                Height = 34,
                Location = new Point(45, 110),
                BackColor = Color.Transparent
            };
            pnlPasswordBorder.Paint += (s, e) =>
            {
                int radius = 6;
                int penWidth = 1;
                Color borderColor = ColorTranslator.FromHtml("#D1D5DB");


                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, pnlPasswordBorder.Width - 1, pnlPasswordBorder.Height - 1);

                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.CloseFigure();

                    using (Pen pen = new Pen(borderColor, penWidth))
                    {
                        pen.LineJoin = LineJoin.Round;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };


            txtPassword = new TextBox
            {
                Width = 270,
                Location = new Point(45, 175),
                UseSystemPasswordChar = true,
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Enabled = false // disabled until username is typed
            };
            pnlAdminLogin.Controls.Add(txtPassword);
            pnlAdminLogin.Controls.Add(pnlPasswordBorder);

            btnLogin = new Button
            {
                Text = "Login",
                Width = 270,
                Height = 38,
                Location = new Point(45, 220),
                BackColor = ColorTranslator.FromHtml("#381313"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            // ✅ Use your existing SetButtonRounded system
            SetButtonRounded(btnLogin, 20); // 10 = corner radius

            // Login logic
            btnLogin.Click += BtnLogin_Click;

            pnlAdminLogin.Controls.Add(btnLogin);

            // Enable password box when username is typed
            txtUsername.TextChanged += (s, e) =>
            {
                txtPassword.Enabled = !string.IsNullOrWhiteSpace(txtUsername.Text);
            };

            // Login button logic with validation
            btnLogin.Click += BtnLogin_Click;

            // Optional: Back button
            Button btnBack = new Button
            {
                Text = "← Back",
                Width = 80,
                Height = 30,
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat,
                ForeColor = ColorTranslator.FromHtml("#381313"),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) =>
            {
                pnlAdminLogin.Visible = false;
                btnAdmin.Enabled = true;
                //btnStudent.Enabled = true;

                foreach (Control ctrl in this.Controls)
                    ctrl.Visible = true;
            };
            pnlAdminLogin.Controls.Add(btnBack);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // --- Check if username is empty first ---
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Please enter your username first.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return; // Stop here
            }

            // --- Check if password is empty ---
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return; // Stop here
            }

            // --- Admin Login ---
            if (username == "admin" && password == "admin")
            {
                // ✅ Hide the PreLogin form (so it won’t show behind MainForm)
                //preLoginForm.Hide();

                this.Hide(); // hide ModeSelectorForm

                // Open MainForm and pass the username
                MainForm mainForm = new MainForm
                {
                    Username = username // make sure MainForm has a public property "Username"
                };
                mainForm.ShowDialog();

                this.Close(); // close ModeSelectorForm after MainForm closes
                return;
            }

            // --- Incorrect credentials ---
            MessageBox.Show("Incorrect username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.Clear(); // clear password
            txtPassword.Focus();  // focus back on password
        }
    }
}
