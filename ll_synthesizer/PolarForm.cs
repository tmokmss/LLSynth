using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ll_synthesizer
{
    partial class PolarForm : Form
    {
        private List<SimpleIcon> iconList = new List<SimpleIcon>();
        private int savedCount;

        public ItemCombiner Ic { set; get; }
        
        public PolarForm()
        {
            InitializeComponent();
        }

        public PolarForm(ItemCombiner ic)
        {
            InitializeComponent();
            Ic = ic;
            ApplySizeToIcons();
            SetIcons();
            RefreshIcons();
        }

        public void RefreshIcons()
        {
            if (savedCount != Ic.GetCount())
            {
                SetIcons();
            }
            ShowIcons();
        }

        public void SetIcons()
        {
            ClearItemList();
            Controls.Clear();
            var count = Ic.GetCount();
            savedCount = count;
            for (var i = 0; i < count; i++)
            {
                var myIcon = new SimpleIcon(Ic.GetItem(i));
                iconList.Add(myIcon);
                Controls.Add(myIcon);
            }
        }

        private void DrawBackground()
        {
            var height = ClientSize.Height;
            var width = ClientSize.Width;

            var g = this.CreateGraphics();
            g.Clear(this.BackColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Pen p = new Pen(Color.LightGray, 1);
            for (var i=0; i<7; i++)
            {
                var radius = (int)(height * Math.Pow(0.7, i));
                var x = width / 2 - radius;
                var y = -radius;

                Rectangle rect = new Rectangle(x, y, radius*2, radius*2);

                g.DrawEllipse(p, rect); 
            }

        }

        private void ShowIcons()
        {
            foreach(var icon in iconList)
            {
                icon.SetPosition();
            }
        }

        private void ApplySizeToIcons()
        {
            var size = ClientSize;
            SimpleIcon.ParentWidth = size.Width;
            SimpleIcon.ParentHeight = size.Height;
            SimpleIcon.BackPanelColor = BackColor;
        }

        private void ClearItemList()
        {
            foreach (var icon in iconList)
            {
                icon.Dispose();
            }
            iconList.Clear();
        }

        private void PolarForm_ResizeEnd(object sender, EventArgs e)
        {
            DrawBackground();
            ApplySizeToIcons();
        }

        int paintcount = 0;
        private void PolarForm_Paint(object sender, PaintEventArgs e)
        {
            if (paintcount > 10)
            {
                DrawBackground();
                paintcount = 0;
            }
            paintcount++;

        }
    }

    class SimpleIcon:PictureBox
    {
        public static int ParentWidth;
        public static int ParentHeight;
        public static Color BackPanelColor;
        private const int OrgHeight = 128;
        private const int OrgWidth = 128;

        int paddingx;
        int paddingy;

        private ItemSet item;
        private bool isDraggable = false;
        private Point MouseDownLocation;
        private int leftmax, topmax;
        private Bitmap OrgBitmap;
        private bool isReversed = false;

        public SimpleIcon(ItemSet item):base()
        {
            this.item = item;

            InitializeIcon();

            paddingx = Width / 2;
            paddingy = Height / 2;

            InitializeEvents();
        }

        private void InitializeIcon()
        {
            var orgIcon = item.MyIcon;
            OrgBitmap = new Bitmap(orgIcon.ImageLocation);
            ZoomOut(1);
        }

        private void InitializeEvents()
        {
            this.MouseDown += SimpleIcon_MouseDown;
            this.MouseMove += SimpleIcon_MouseMove;
            this.MouseUp += SimpleIcon_MouseUp;
        }

        private void ZoomOut(double ratio)
        {
            ratio = Math.Abs(ratio);
            var newWidth = (int)(OrgWidth * (ratio / 2 + 0.5));
            var newHeight = (int)(OrgHeight * (ratio / 2 + 0.5));

            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(newWidth, newHeight);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);

            //Bitmapオブジェクトの作成
            Bitmap image = OrgBitmap.Clone(new Rectangle(0, 0, OrgBitmap.Width, OrgBitmap.Height), OrgBitmap.PixelFormat);
            //補間方法として高品質双三次補間を指定する
            g.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //画像を縮小して描画する
            g.DrawImage(image, 0, 0, newWidth, newHeight);

            PaintMask(g);

            //BitmapとGraphicsオブジェクトを破棄
            image.Dispose();
            g.Dispose();

            Width = newWidth;
            Height = newHeight;

            //PictureBox1に表示する
            Image = canvas;
        }

        private void SimpleIcon_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggable = false;
        }

        private void SimpleIcon_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggable) return;
            var left = e.X + Left - MouseDownLocation.X;
            var top = e.Y + Top - MouseDownLocation.Y;
            if (left < 0) left = 0;
            else if (left > leftmax) left = leftmax;
            if (top < 0) top = 0;
            else if (top > topmax) top = topmax;
            Left = left;
            Top = top;

            leftmax = ParentWidth - Width;
            topmax = ParentHeight - Height;

            ApplyFactors();
        }

        private void SimpleIcon_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggable = true;
            MouseDownLocation = e.Location;
            leftmax = ParentWidth - OrgWidth;
            topmax = ParentHeight - OrgHeight;
            if (e.Button == MouseButtons.Middle)
            {
                isReversed = !isReversed;
                ApplyFactors();
            }
            else if (e.Button == MouseButtons.Right)
            {
                item.Muted = !item.Muted;
                ApplyFactors();
            }
        }

        public void SetPosition()
        {
            var maxmins = item.MaxMins;
            var lr = (double)(item.LRBalance - maxmins[1]) / (maxmins[0] - maxmins[1]);
            var amp = (double)(item.TotalFactor) / (maxmins[2]);
            SetXYCoordinates(lr, amp);
            ZoomOut(amp);
        }

        /// <summary>
        /// 各種係数からXY座標位置を計算
        /// </summary>
        /// <param name="lrBalance">LRBalance -1 to 1</param>
        /// <param name="totalFactor">TotalFactor 0 to 1</param>
        private void SetXYCoordinates(double lrBalance, double totalFactor)
        {
            var xSize = (int)(ParentWidth - 2 * paddingx);
            var ySize = (int)(ParentHeight - 2 * paddingy);
            var xPoint = (int)(xSize * lrBalance);
            var yPoint = ySize - (int)((ySize * (1 - Math.Abs(totalFactor))));
            Left = xPoint;
            Top = yPoint;
        }

        private void ApplyFactors()
        {
            item.LRBalance = CalcLRBalance();
            item.TotalFactor = CalcTotalFactor();
        }

        private int CalcLRBalance()
        {
            var xSize = ParentWidth - 2 * paddingx;
            var lr = Left * 1.0 / xSize;
            var maxmins = item.MaxMins;
            return (int)(lr * (maxmins[0] - maxmins[1]) + maxmins[1]);
        }

        private int CalcTotalFactor()
        {
            var ySize = (int)(ParentHeight - 2 * paddingy);
            var amp = 1 - (ySize - Bottom) * 1.0 / ySize;
            ZoomOut(amp);
            var maxmins = item.MaxMins;
            var factor = (isReversed) ? -1 : 1;
            return (int)(amp * maxmins[2]) * factor;
        }
        
        void PaintMask(Graphics g)
        {
            if (isReversed)
            {
                Font fnt = new System.Drawing.Font("Meiryo UI", 15, FontStyle.Bold); ;
                g.DrawString("Rev", fnt, Brushes.Black, 0, 0);
            }
            if (item.Muted)
            {
                var alpha = 210;
                Color color = Color.FromArgb(alpha, BackPanelColor);
                Brush b = new SolidBrush(color);
                g.FillRectangle(b, 0, 0, Width, Height);
            }
        }
    }
}
