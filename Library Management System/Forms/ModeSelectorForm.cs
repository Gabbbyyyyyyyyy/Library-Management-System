using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Library_Management_System.Forms;
using System.Runtime.InteropServices;


namespace Library_Management_System
{
    public partial class ModeSelectorForm : Form
    {

        private PreLoginButtons preLoginForm;
        private Panel pnlAdminLogin;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Timer rollDownTimer;
        private int targetHeight = 280;
        // animation helpers (add near other private fields)
        private int animationStep = 0;
        private readonly int animationMaxSteps = 30; // how many ticks the animation runs for (larger = slower)
        private int startWidth = 0;
        private int startHeight = 0;
        private int targetWidth = 300;




        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool AnimateWindow(IntPtr hWnd, int dwTime, int dwFlags);

        // Animation flags
        private const int AW_HOR_POSITIVE = 0x0001;   // Left-to-right
        private const int AW_HOR_NEGATIVE = 0x0002;   // Right-to-left
        private const int AW_VER_POSITIVE = 0x0004;   // Top-to-bottom
        private const int AW_VER_NEGATIVE = 0x0008;   // Bottom-to-top
        private const int AW_CENTER = 0x0010;         // Expand from center
        private const int AW_HIDE = 0x10000;          // Hide window
        private const int AW_ACTIVATE = 0x20000;      // Show window
        private const int AW_SLIDE = 0x40000;         // Sliding animation
        private const int AW_BLEND = 0x80000;         // Fade effect
       

        // ✅ Constructor now receives the PreLoginButtons reference
        public ModeSelectorForm(PreLoginButtons preLoginForm)
        {
            InitializeComponent();
            this.preLoginForm = preLoginForm; // store the reference

            this.StartPosition = FormStartPosition.CenterParent; // Center modal
                                                                 //this.BackColor = System.Drawing.ColorTranslator.FromHtml("#f7ebdd");

            // In your constructor or Form_Load
            this.KeyPreview = true; // allow form to receive key events
            this.KeyDown += ModeSelectorForm_KeyDown;

            // Set default button color and style
            btnAdmin.ForeColor = ColorTranslator.FromHtml("#381313");
            btnAdmin.BackColor = ColorTranslator.FromHtml("#f2dabe");
            //btnAdmin.ForeColor = Color.White;
            btnAdmin.FlatStyle = FlatStyle.Flat;
            btnAdmin.FlatAppearance.BorderSize = 0;

            btnStudent.FlatStyle = FlatStyle.Flat;
            btnStudent.FlatAppearance.BorderSize = 0;
            btnStudent.BackColor = Color.Transparent;
            btnStudent.ForeColor = ColorTranslator.FromHtml("#381313");


            // Apply rounded edges
            SetButtonRounded(btnAdmin, 20); // 20 = corner radius
            SetButtonRounded(btnStudent, 20); // 20 = corner radius

            // 👇 Add custom paint handler for student button border
            btnStudent.Paint += (s, e) => DrawRoundedBorder(btnStudent, e.Graphics, 20);

            // 👇 Initialize admin login panel (hidden)
            CreateAdminLoginPanel();
        }
        // Then define the handler:
        // Enter key handling moved to the form level
        private void ModeSelectorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (pnlAdminLogin.Visible && e.KeyCode == Keys.Enter)
            {
                BtnLogin_Click(btnLogin, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
        }



        private void SetButtonRounded(Button btn, int radius)
        {
            // Make sure system doesn't draw its own border
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TabStop = false; // prevents focus dotted rectangle (optional)

            // Recreate region whenever size changes
            btn.Resize -= Btn_Resize; // avoid duplicate handlers
            btn.Resize += Btn_Resize;

            // Paint custom border
            btn.Paint -= Btn_Paint;
            btn.Paint += Btn_Paint;

            // Apply initial region now
            ApplyRoundedRegion(btn, radius);
        }
        private void Btn_Resize(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
                ApplyRoundedRegion(btn, 20); // or store radius per-button if needed
        }

        private void ApplyRoundedRegion(Button btn, int radius)
        {
            Rectangle rect = btn.ClientRectangle;
            int diameter = radius * 2;

            using (GraphicsPath path = new GraphicsPath())
            {
                // Use RectangleF to be precise and avoid clipping
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                // Set region from path (region will clip the control shape perfectly)
                btn.Region = new Region(path);
            }
        }
        private void Btn_Paint(object sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            int radius = 20;                 // same radius as before
            float penWidth = 1.4f;           // slightly thicker for smoother edges
            Color borderColor = Color.FromArgb(120, 56, 19, 19); // rich but soft brown

            // Enable highest-quality smoothing
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Draw slightly inside the button bounds
            RectangleF rect = new RectangleF(
                penWidth / 2f,
                penWidth / 2f,
                btn.ClientSize.Width - penWidth - 1f,
                btn.ClientSize.Height - penWidth - 1f
            );

            float diameter = radius * 2f;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                using (Pen pen = new Pen(borderColor, penWidth))
                {
                    pen.LineJoin = LineJoin.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void DrawRoundedBorder(Button btn, Graphics g, int radius)
        {
            float penWidth = 1.4f;
            Color borderColor = Color.FromArgb(120, 56, 19, 19);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            RectangleF rect = new RectangleF(
                penWidth / 2f,
                penWidth / 2f,
                btn.ClientSize.Width - penWidth - 1f,
                btn.ClientSize.Height - penWidth - 1f
            );

            float diameter = radius * 2f;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
                path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
                path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                using (Pen pen = new Pen(borderColor, penWidth))
                {
                    pen.LineJoin = LineJoin.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;

                    g.DrawPath(pen, path);
                }
            }
        }



        private void ModeSelectorForm_Load(object sender, EventArgs e)
        {
            // Start with 0 opacity for smooth fade transition
            this.Opacity = 0;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Show the form before animation starts
            this.Show();
            this.Refresh();

            // 🎞️ Combine slide down + fade-in like STI eLMS
            AnimateWindow(this.Handle, 500, AW_VER_POSITIVE | AW_SLIDE | AW_ACTIVATE | AW_BLEND);

            // Fully visible after animation
            this.Opacity = 1;
        }
    




private void btnStudent_Click_1(object sender, EventArgs e)
        {
            // ✅ Hide the PreLoginButtons form
            preLoginForm.Hide();

            // Open LoginForm
            LoginForm loginForm = new LoginForm();

            // Optional: show PreLoginButtons again after LoginForm closes
            loginForm.FormClosed += (s, args) => preLoginForm.Show();

            loginForm.Show();

            // Close ModeSelectorForm
            this.Close();
        }

        // 🎞️ Animate roll-down panel for admin login
        // 🎞️ Roll-down + Replace content
        private void btnAdmin_Click_1(object sender, EventArgs e)
        {
            btnAdmin.Enabled = false;
            btnStudent.Enabled = false;

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
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
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
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml("#381313"),
                Location = new Point(45, 85),
                AutoSize = true
            };
            pnlAdminLogin.Controls.Add(lblUser);

            txtUsername = new TextBox
            {
                Width = 270,
                Location = new Point(45, 110),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10)
            };
            pnlAdminLogin.Controls.Add(txtUsername);

            Label lblPass = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10),
                ForeColor = ColorTranslator.FromHtml("#381313"),
                Location = new Point(45, 150),
                AutoSize = true
            };
            pnlAdminLogin.Controls.Add(lblPass);

            txtPassword = new TextBox
            {
                Width = 270,
                Location = new Point(45, 175),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Enabled = false // disabled until username is typed
            };
            pnlAdminLogin.Controls.Add(txtPassword);

            btnLogin = new Button
            {
                Text = "Login",
                Width = 270,
                Height = 38,
                Location = new Point(45, 220),
                BackColor = ColorTranslator.FromHtml("#381313"),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.FlatAppearance.BorderSize = 0;
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
                btnStudent.Enabled = true;

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
                MessageBox.Show("Login successful!", "Welcome", MessageBoxButtons.OK, MessageBoxIcon.Information);

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







        private void btnRegister_Click_1(object sender, EventArgs e)
        {
            preLoginForm.Hide();

            RegisterForm registerForm = new RegisterForm();
            registerForm.FormClosed += (s, args) => preLoginForm.Show();

            registerForm.Show();

            this.Close();
        }
    }
}
