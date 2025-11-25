using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Library_Management_System.Models
{
    public static class ControlExtensions
    {
        public static void ApplyRoundedCorners(this Control control, int radius, Color clickBorderColor, int borderWidth = 2)
        {
            bool isClicked = false;

            // Set the initial region
            control.Region = CreateRoundedRegion(control, radius);

            // Update region when the control is resized
            control.SizeChanged += (s, e) =>
            {
                control.Region = CreateRoundedRegion(control, radius);
                control.Invalidate();
            };

            // Paint event to optionally draw rounded border
            control.Paint += (s, e) =>
            {
                if (isClicked)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using (GraphicsPath path = CreateRoundedPath(control, radius))
                    using (Pen pen = new Pen(clickBorderColor, borderWidth))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };

            // Mouse events to toggle clicked state
            control.MouseDown += (s, e) =>
            {
                isClicked = true;
                control.Invalidate(); // redraw with border
            };

            control.MouseUp += (s, e) =>
            {
                isClicked = false;
                control.Invalidate(); // redraw without border
            };
        }

        private static Region CreateRoundedRegion(Control control, int radius)
        {
            using (GraphicsPath gp = CreateRoundedPath(control, radius))
            {
                return new Region(gp);
            }
        }

        private static GraphicsPath CreateRoundedPath(Control control, int radius)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddArc(0, 0, radius, radius, 180, 90); // Top-left
            gp.AddArc(control.Width - radius, 0, radius, radius, 270, 90); // Top-right
            gp.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90); // Bottom-right
            gp.AddArc(0, control.Height - radius, radius, radius, 90, 90); // Bottom-left
            gp.CloseFigure();
            return gp;
        }
    }
}