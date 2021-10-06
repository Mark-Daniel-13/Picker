using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Picker
{
    public partial class PickerMenu : Form
    {
        bool isStart = false;
        const int MYACTION_HOTKEY_ID = 1;//start
        const int MYACTION_HOTKEY_ID_MOVEPSTN_LEFT = 2;//move position
        const int MYACTION_HOTKEY_ID_MOVEPSTN_RIGHT = 3;//move position
        const int MYACTION_HOTKEY_ID_SPIDER = 4;//spider
        int currentPosition = 2; //1 = left , 2 middle , 3 right
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        // DLL libraries used to manage hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        public PickerMenu()
        {
            InitializeComponent();
            // Modifier keys codes: Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            // Compute the addition of each combination of the keys you want to be pressed
            // ALT+CTRL = 1 + 2 = 3 , CTRL+SHIFT = 2 + 4 = 6...
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_MOVEPSTN_LEFT, 1, (int)Keys.Left);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_MOVEPSTN_RIGHT, 1, (int)Keys.Right);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID, 1, (int)Keys.NumPad0);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_SPIDER, 1, (int)Keys.NumPad1);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                isStart = !isStart;
                if (!isStart)
                {
                    timer1.Start();
                    btnStart.Text = "Cancel";
                }
                else
                {
                    timer1.Stop();
                    btnStart.Text = "Start";
                }
            } else if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_MOVEPSTN_LEFT) {
                currentPosition = (currentPosition != 1) ? currentPosition - 1 : 1;
            }else if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_MOVEPSTN_RIGHT){
                currentPosition = (currentPosition != 3) ? currentPosition + 1 : 3;
            }
            else if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_SPIDER) {
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(100, 100);
            }
            base.WndProc(ref m);
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

            var p = Process.GetProcessesByName("BinamonRunner");
            if (p != null)
            {
                IntPtr h = p[0].MainWindowHandle;
                SetForegroundWindow(h);
                int yellow = c.R - c.G;
                if ((yellow < 25 && c.B<100) && c.R > 150 && c.G > 150)
                {
                    label1.Text = "Yellow";
                    //SendKeys.SendWait("d");
                }
                else if (c.R > c.B && c.R > c.G)
                {
                    label1.Text = "Red";
                    //SendKeys.SendWait("s");
                }
                else if (c.G > c.B && c.G > c.R)
                {
                    label1.Text = "Green";
                    //SendKeys.SendWait("a");
                }
                else if (c.R<c.B && c.G < c.B)
                {
                    label1.Text = "Blue";
                    //SendKeys.SendWait("w");
                }
                else {
                    label1.Text = "N/A";
                }
                label2.Text = string.Format("{0}-{1}-{2}", c.R, c.G, c.B);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isStart)
            {
                timer1.Start();
                isStart = true;
                btnStart.Text = "Cancel";
            }
            else
            {
                timer1.Stop();
                isStart = false;
                btnStart.Text = "Start";
            }
        }
    }
}
