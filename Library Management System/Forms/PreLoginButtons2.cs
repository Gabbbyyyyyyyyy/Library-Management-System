using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Library_Management_System.Models;

namespace Library_Management_System.Forms
{
    public partial class PreLoginButtons2 : Form
    {
        public PreLoginButtons2()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            // Button styling
            button1.BackColor = ColorTranslator.FromHtml("#381313");
            button1.ForeColor = Color.White;
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;

            SetButtonRounded(button1, 20);
        }

        private void SetButtonRounded(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = btn.ClientRectangle;
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

            path.CloseFigure();
            btn.Region = new Region(path);
        }

        private void PreLoginButtons2_Load(object sender, EventArgs e)
        {
            button1.BackColor = ColorTranslator.FromHtml("#381313");
            button1.ForeColor = Color.White;

            button2.ForeColor = ColorTranslator.FromHtml("#483528");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ModeSelectorForm2 selector = new ModeSelectorForm2();
            selector.StartPosition = FormStartPosition.CenterParent;
            selector.ShowDialog(this); // ← THIS centers the modal properly
        }



        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "This feature will be available soon!",
                "Coming Soon",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}
