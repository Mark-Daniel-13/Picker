using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Picker
{
    public partial class PickerMenu : Form
    {
        bool isStart = false;
        const int MYACTION_HOTKEY_ID_W = 26;//start
        const int MYACTION_HOTKEY_ID_A = 25;//start
        const int MYACTION_HOTKEY_ID_S = 24;//start
        const int MYACTION_HOTKEY_ID_D = 23;//start
        const int MYACTION_HOTKEY_ID_MOVEPSTN_LEFT = 2;//move position
        const int MYACTION_HOTKEY_ID_MOVEPSTN_RIGHT = 3;//move position
        const int MYACTION_HOTKEY_ID_SPIDER = 4;//spider
        const int MYACTION_HOTKEY_ID_WOLF = 5;//SMALL WOLF
        const int MYACTION_HOTKEY_ID_WOLFB = 6;//BIG WOLF
        const int MYACTION_HOTKEY_ID_BEARS = 7;//BEAR
        const int MYACTION_HOTKEY_ID_OGRE = 8;//OGRE
        const int MYACTION_HOTKEY_ID_GIANTS = 9;//GIANTS
        const int MYACTION_HOTKEY_ID_COLLOSSUS = 10;//COLLOSSUS
        const int MYACTION_HOTKEY_ID_BOSS = 11;//BOSS
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

        private List<CordsModel> monsterCords = new List<CordsModel>();
        CordsModel selectedCords;
        public PickerMenu()
        {
            InitializeComponent();
            // Modifier keys codes: Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            // Compute the addition of each combination of the keys you want to be pressed
            // ALT+CTRL = 1 + 2 = 3 , CTRL+SHIFT = 2 + 4 = 6...
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_MOVEPSTN_LEFT, 1, (int)Keys.Left);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_MOVEPSTN_RIGHT, 1, (int)Keys.Right);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_W, 4,(int)Keys.Space);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_A, 4, (int)Keys.A);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_S, 4, (int)Keys.S);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_D, 4, (int)Keys.D);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_WOLF, 1, (int)Keys.NumPad1);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_WOLFB, 1, (int)Keys.NumPad2);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_SPIDER, 1, (int)Keys.NumPad3);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_BEARS, 1, (int)Keys.NumPad4);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_OGRE, 1, (int)Keys.NumPad5);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_GIANTS, 1, (int)Keys.NumPad6);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_COLLOSSUS, 1, (int)Keys.NumPad7);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_BOSS, 1, (int)Keys.NumPad8);

            //Wolf
            monsterCords.Add(new CordsModel { ModelId = 1, X = 973, Y = 369 });//SmallWolf CD
            monsterCords.Add(new CordsModel { ModelId = 2, X = 972, Y = 368 });//BigWolf CD
            monsterCords.Add(new CordsModel { ModelId = 3, X = 983, Y = 283 });//Spider CD
            monsterCords.Add(new CordsModel { ModelId = 4, X = 973, Y = 313 });//Bears CD
            monsterCords.Add(new CordsModel { ModelId = 5, X = 985, Y = 245 });//Ogre CD
            monsterCords.Add(new CordsModel { ModelId = 6, X = 977, Y = 292 });//Giants CD
            monsterCords.Add(new CordsModel { ModelId = 7, X = 980, Y = 395 });//Colossus
            monsterCords.Add(new CordsModel { ModelId = 8, X = 980, Y = 395 });//BOSS??
        }

        protected override void WndProc(ref Message m)
        {
            if ((m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_W) || (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_S) || (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_A) || (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_D))
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
            }else if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_WOLF) { //SMALL WOLF
                selectedCords = monsterCords.Where(m=>m.ModelId == 1).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_WOLFB) { // BIG WOLF
                selectedCords = monsterCords.Where(m => m.ModelId == 2).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_SPIDER) { // SPIDER
                selectedCords = monsterCords.Where(m => m.ModelId == 3).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_BEARS) { // BEARS
                selectedCords = monsterCords.Where(m => m.ModelId == 4).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_OGRE) { 
                selectedCords = monsterCords.Where(m => m.ModelId == 5).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_GIANTS) { 
                selectedCords = monsterCords.Where(m => m.ModelId == 6).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_COLLOSSUS) { 
                selectedCords = monsterCords.Where(m => m.ModelId == 7).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            else if(m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_BOSS) { 
                selectedCords = monsterCords.Where(m => m.ModelId == 8).FirstOrDefault();
                this.Cursor = new Cursor(Cursor.Current.Handle);
                Cursor.Position = new Point(selectedCords.X, selectedCords.Y);
            }
            //label3.Text = string.Format("{0}   {1}", MousePosition.X, MousePosition.Y);
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
                int yellow = c.G- c.R;
                if ((yellow < 25) && ((c.R > 200 && c.G > 200 && c.B<150) || (c.B < 100 && c.R>c.B && c.G>c.B)) && yellow>-30)
                {
                    label1.Text = "Yellow";
                    SendKeys.SendWait("d");
                }
                else if (c.R > c.B && c.R > c.G)
                {
                    label1.Text = "Red";
                    SendKeys.SendWait("s");
                }
                else if (c.G > c.B && c.G > c.R)
                {
                    label1.Text = "Green";
                    SendKeys.SendWait("a");
                }
                else if (c.R<c.B && c.G < c.B)
                {
                    label1.Text = "Blue";
                    SendKeys.SendWait("w");
                }
                else {
                    label1.Text = "N/A";
                }

                Random rand = new Random();
                int newTickVal = rand.Next(450,600);
                timer1.Interval = newTickVal;
                label3.Text = newTickVal.ToString();
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
