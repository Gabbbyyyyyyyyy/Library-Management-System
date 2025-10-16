using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Library_Management_System.Models;

namespace Library_Management_System.Forms
{
    public partial class PreLoginButtons : Form
    {
        
        public PreLoginButtons()
        {

            InitializeComponent();

            this.Dock = DockStyle.Fill;
            this.Size = Screen.PrimaryScreen.Bounds.Size;
            this.Location = Screen.PrimaryScreen.Bounds.Location;

            //this.StartPosition = FormStartPosition.CenterScreen;


            // Set default button color and style
            button1.BackColor = ColorTranslator.FromHtml("#381313");
            button1.ForeColor = Color.White;
            button1.FlatStyle = FlatStyle.Flat;
            button1.FlatAppearance.BorderSize = 0;

            // Apply rounded edges
            SetButtonRounded(button1, 20); // 20 = corner radius


        }

        private void SetButtonRounded(Button btn, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = btn.ClientRectangle;
            int diameter = radius * 2;

            // Top-left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Top-right
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Bottom-right
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Bottom-left
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            btn.Region = new Region(path);
        }


        private void PreLoginButtons_Load(object sender, EventArgs e)
        {
            // Set button color when form loads
            button1.BackColor = System.Drawing.ColorTranslator.FromHtml("#381313");
            button1.ForeColor = System.Drawing.Color.White;
            button2.ForeColor = ColorTranslator.FromHtml("#483528");

            //panel1.BackColor = ColorTranslator.FromHtml("#f7ebdd");

        }





        private void button1_Click_1(object sender, EventArgs e)
        {
            
            using (ModeSelectorForm selector = new ModeSelectorForm(this))
            {
                selector.ShowDialog(); // Opens as modal window
            }   
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "This feature will be available soon!",     // message
                "Coming Soon",                              // title
                MessageBoxButtons.OK,                       // button type
                MessageBoxIcon.Information                  // icon
            );
        }

    }
}