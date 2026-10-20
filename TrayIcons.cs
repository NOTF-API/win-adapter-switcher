using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WinAdapterSwitcher
{
    internal static class TrayIcons
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon Create(bool? wiredEnabled, bool? wirelessEnabled)
        {
            using (Bitmap bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                using (GraphicsPath background = RoundedRect(new Rectangle(1, 1, 30, 30), 8))
                using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(255, 55, 65, 81)))
                using (Pen backgroundPen = new Pen(Color.FromArgb(255, 17, 24, 39), 1f))
                {
                    graphics.FillPath(backgroundBrush, background);
                    graphics.DrawPath(backgroundPen, background);
                }

                DrawBar(graphics, new Rectangle(6, 7, 8, 18), wiredEnabled);
                DrawBar(graphics, new Rectangle(18, 7, 8, 18), wirelessEnabled);

                IntPtr handle = bitmap.GetHicon();
                try
                {
                    using (Icon temporary = Icon.FromHandle(handle))
                    {
                        return (Icon)temporary.Clone();
                    }
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        private static void DrawBar(Graphics graphics, Rectangle bounds, bool? enabled)
        {
            Color fill;
            Color border;
            if (!enabled.HasValue)
            {
                fill = Color.FromArgb(255, 245, 158, 11);
                border = Color.FromArgb(255, 180, 83, 9);
            }
            else if (enabled.Value)
            {
                fill = Color.FromArgb(255, 34, 197, 94);
                border = Color.FromArgb(255, 21, 128, 61);
            }
            else
            {
                fill = Color.FromArgb(255, 156, 163, 175);
                border = Color.FromArgb(255, 75, 85, 99);
            }

            using (GraphicsPath path = RoundedRect(bounds, 3))
            using (SolidBrush brush = new SolidBrush(fill))
            using (Pen pen = new Pen(border, 1f))
            {
                graphics.FillPath(brush, path);
                graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
