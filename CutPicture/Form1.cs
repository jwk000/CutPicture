using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace CutPicture
{
    public partial class Form1 : Form
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }
        public Form1()
        {
            InitializeComponent();
            
        }


        int hotkey = (int)(KeyModifier.Alt | KeyModifier.Control);
        private void Form1_Load(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Hide();

            RegisterHotKey(this.Handle, 10086, hotkey, (int)Keys.C);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            UnregisterHotKey(this.Handle, 10086);
        }

        Form f = new Form();
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    if(m.WParam.ToString() == "10086")
                    {
                        if(cutState== eCutStatus.None)
                        {
                            cutState = eCutStatus.Ready;
                            Bitmap catchScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                            Graphics g = Graphics.FromImage(catchScreen);
                            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.PrimaryScreen.Bounds.Size);
                            this.FormBorderStyle = FormBorderStyle.None;
                            this.WindowState = FormWindowState.Maximized;
                            this.BackgroundImage = catchScreen;
                            this.Show();

                        }
                    }
                    break;
                default:
                    break;
            }
        }

        enum eCutStatus
        {
            None,Ready,Begin,End
        }

        eCutStatus cutState = eCutStatus.None;
        Rectangle cutRect = new Rectangle();
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (cutState != eCutStatus.Ready) return;
            if(e.Button == MouseButtons.Left)
            {
                cutRect.X = e.X;
                cutRect.Y = e.Y;
                cutRect.Width = 0;
                cutRect.Height = 0;
                cutState = eCutStatus.Begin;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (cutState != eCutStatus.Begin) return;
            if (e.Button == MouseButtons.Left)
            {
                cutRect.Width = e.X - cutRect.X;
                cutRect.Height = e.Y - cutRect.Y;
                cutState = eCutStatus.End;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.X > cutRect.Left && e.X < cutRect.Right && e.Y > cutRect.Top && e.Y < cutRect.Bottom)
            {
                Bitmap cutPic = new Bitmap(cutRect.Width, cutRect.Height);
                Graphics g = Graphics.FromImage(cutPic);
                g.DrawImage(this.BackgroundImage, new Rectangle(0, 0, cutRect.Width, cutRect.Height), cutRect, GraphicsUnit.Pixel);
                Clipboard.SetImage(cutPic);
                g.Dispose();
                cutPic.Dispose();

                cutState = eCutStatus.None;
                cutRect.Width = 0;
                cutRect.Height = 0;
                this.Hide();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if(e.Button == MouseButtons.Right)
            {
                cutState = eCutStatus.None;
                cutRect.Width = 0;
                cutRect.Height = 0;
                this.Hide();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(cutState == eCutStatus.Begin)
            {
                cutRect.Width = e.X - cutRect.X;
                cutRect.Height = e.Y - cutRect.Y;
                Invalidate();
            }
        }

        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            if(cutState != eCutStatus.None)
            {
                Color backcolor = Color.FromArgb(155, Color.Black);
                List<Rectangle> rects = new List<Rectangle>();
                rects.Add(new Rectangle(0, 0, this.BackgroundImage.Width, cutRect.Y));
                rects.Add(new Rectangle(0, cutRect.Y, cutRect.X, cutRect.Height));
                rects.Add(new Rectangle(cutRect.Right, cutRect.Top, this.BackgroundImage.Width - cutRect.Right, cutRect.Height));
                rects.Add(new Rectangle(0, cutRect.Bottom, this.BackgroundImage.Width, this.BackgroundImage.Height - cutRect.Bottom));
                g.FillRectangles(new SolidBrush(backcolor), rects.ToArray());
            }


            if(cutState == eCutStatus.Begin||cutState== eCutStatus.End)
            {
                g.DrawRectangle(Pens.Red, cutRect);
            }
        }
    }
}
