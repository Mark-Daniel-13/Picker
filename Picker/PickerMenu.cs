using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace Picker
{
    public partial class PickerMenu : Form
    {
        bool isStart = false;
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        public PickerMenu()
        {
            InitializeComponent();
        }

        Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        public Color GetColorAt(Point location)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return screenPixel.GetPixel(0, 0);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point cursor = new Point();
            GetCursorPos(ref cursor);

            var c = GetColorAt(cursor);
            this.BackColor = c;
            if (c.B == 0 && c.R>0 && c.G>0) {
                label1.Text = "Yellow";
                SendKeys.Send("{RIGHT}");
            }
            else if (c.R > c.B && c.R > c.G)
            {
                label1.Text = "Red";
                SendKeys.Send("{DOWN}");
            }
            else if (c.G > c.B && c.G > c.R)
            {
                label1.Text = "Green";
                SendKeys.Send("{LEFT}");
            }
            else if(c.B > c.R && c.B > c.G)
            {
                label1.Text = "Blue";
                SendKeys.Send("{UP}");
            }
            label2.Text = string.Format("{0}-{1}-{2}", c.R, c.G, c.B);

            Random rand = new Random();
            int newVal = rand.Next(300, 800);
            timer1.Interval = newVal;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isStart)
            {
                timer1.Start();
                isStart = true;
                btnStart.Text = "Cancel";
            }
            else {
                timer1.Stop();
                isStart = false;
                btnStart.Text = "Start";
            }
        }
    }
}
