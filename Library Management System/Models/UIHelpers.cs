using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class UIHelpers
{
    public static void ApplyCardStyle(Panel panel, int radius = 10)
    {
        panel.BackColor = Color.White;
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

                // Background
                using (SolidBrush brush = new SolidBrush(panel.BackColor))
                    e.Graphics.FillPath(brush, path);

                // Subtle shadow
                using (Pen shadow = new Pen(Color.FromArgb(230, 230, 230), 2))
                    e.Graphics.DrawPath(shadow, path);
            }
        };
    }
}
