using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace ll_synthesizer
{
    class ItemSet: GraphPanel
    {
        private WavData wd;
        private static WavPlayer wp;
        private static Form1 form;
        private static ItemCombiner ic;

        private static int plotNum = 1000;
        private static double span = 1;
        private static double center = 0.5;
        private static readonly double nodivbyzero = 0.000000000000001;
        private static int fadeTimeMs = 1000;

        public event EventHandler PlotRefreshed;
        public event EventHandler Suicided;
        public event EventHandler FactorChanged;
        delegate void upDownDelegate(double value);
        delegate void booleanDelegate(bool iswhat);

        private bool offsetAdjusted = false;
        private int waitTimeLR; // sleep time of factor fading thread
        private int waitTimeTF;
        private bool userInterruptedLR = false;
        private bool userInterruptedTF = false;

        private Thread LRThread;
        private Thread TFThread;

        static public int FadingTime
        {
            set { fadeTimeMs = value; }
        }

        public bool Muted
        {
            get { return mute.Checked; }
        }

        public bool DSPEnabled
        {
            set { DSPEnable.Checked = value; }
            get { return wd.DSPEnabled; }
        }

        public bool OffsetAdjusted
        {
            get { return offsetAdjusted; }
            set { offsetAdjusted = value; }
        }

        public int LRBalance
        {
            set
            {
                if (value > lrBalance.Maximum)
                    lrBalance.Value = lrBalance.Maximum;
                else if (value < lrBalance.Minimum)
                    lrBalance.Value = lrBalance.Minimum;
                else
                    lrBalance.Value = value;
            }
            get { return lrBalance.Value; }
        }

        public int TotalFactor
        {
            set
            {
                if (value > lrBalance.Maximum)
                    totalFactor.Value = totalFactor.Maximum;
                else if (value < totalFactor.Minimum)
                    totalFactor.Value = totalFactor.Minimum;
                else
                    totalFactor.Value = value;
            }
            get { return totalFactor.Value; }
        }

        public ItemSet(WavData wd): base(wd.GetName())
        {
            this.wd = wd;
            if (wd.isMP3()) this.PlotEnable = true;
            SetInit();
        }

        public ItemSet(String path): this(new WavData(path))
        {
        }

        static public void SetForm(Form1 form)
        {
            ItemSet.form = form;
        }

        static public void SetCombiner(ItemCombiner ic)
        {
            ItemSet.ic = ic;
        }

        protected override void Dispose(bool disposing)
        {
            wd.Dispose();
            wd = null;
            if (LRThread != null)
            {
                LRThread.Abort();
                LRThread = null;
            }
            if (TFThread != null)
            {
                TFThread.Abort();
                TFThread = null;
            }
            form.KeyPushed -= HandleKey;
            base.Dispose(disposing);
        }

        void SetInit()
        {
            PlotLR();
            playButton.Click += new System.EventHandler(this.playButton_Clicked);
            offsetUpDown.ValueChanged += new System.EventHandler(this.OffsetChanged);
            chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.WheelRotated);
            chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ChartClicked);
            icon.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintMask);
            icon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.iconClicked);
            mute.CheckedChanged += new System.EventHandler(this.muteChanged);
            lrBalance.ValueChanged += new System.EventHandler(this.balanceChanged);
            lrBalance.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LRBalance_MouseDown);
            lrBalance.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LRBalance_MouseUp);
            DSPEnable.CheckedChanged += new System.EventHandler(this.ToggleDPS);
            form.KeyPushed += new KeyEventHandler(this.HandleKey);
            totalFactor.ValueChanged += new System.EventHandler(this.factorChanged);
            totalFactor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TotalFactor_MouseDown);
            totalFactor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TotalFactor_MouseUp);
            totalFactor.Value = factorDefault;
        }

        public void PlotLR()
        {
            chart.Series.Clear();
            if (plotEnable)
                AddLR(wd, plotNum, center - span / 2, center + span / 2);
        }

        public void SetChartRegion(double span, double center)
        {
            ItemSet.span = span;
            ItemSet.center = center;
        }

        public int GetLength()
        {
            return wd.GetLength();
        }

        static public void SetWavPlayer(WavPlayer wp)
        {
            ItemSet.wp = wp;
        }

        public WavData GetData() {
            return this.wd;
        }

        public int[] GetFacsMaxMin()
        {
            int[] facs = new int[4];
            facs[0] = lrBalance.Minimum;
            facs[1] = lrBalance.Maximum;
            facs[2] = totalFactor.Minimum;
            facs[3] = totalFactor.Maximum;
            return facs;
        }

        public void Play()
        {
            wp.Play(wd);
        }

        public void SetOffset(double offsetInMs)
        {
            wd.SetOffset(offsetInMs);
            offsetAdjusted = false;
            offsetUpDown.BeginInvoke(new upDownDelegate(changeUpDownValue), new object[] { offsetInMs });
        }

        public void SetOffset(int offset)
        {
            wd.Offset = 0;
            double offsetInMs = wd.IdxToTime(offset) * 1000;
            wd.Offset = offset;
            offsetAdjusted = false;
            offsetUpDown.BeginInvoke(new upDownDelegate(changeUpDownValue), new object[] { offsetInMs });
        }

        public void PrepareAdjustOffset()
        {
            wd.IsDefault = true;
        }

        public void BackToPreparation()
        {
            wd.IsDefault = false;
        }

        void changeUpDownValue(double offset)
        {
            if ((decimal)offset > offsetUpDown.Maximum)
            {
                offsetUpDown.Maximum = (decimal)offset;
            }
            else if ((decimal)offset < offsetUpDown.Minimum)
            {
                offsetUpDown.Minimum = (decimal)offset;
            }
            offsetUpDown.Value = (decimal)offset;
        }

        private void PM1LRBalance(bool isIncrement)
        {
            if (isIncrement)
                LRBalance += 1;
            else
                LRBalance -= 1;
        }

        private void PM1TotalFactor(bool isIncrement)
        {
            if (isIncrement)
            {
                if (TotalFactor == -1)
                {
                    //TotalFactor += 2;
                    //return;
                }
                TotalFactor += 1;
            }
            else
            {
                if (TotalFactor == 1)
                {
                    //TotalFactor -= 2;
                    //return;
                }
                TotalFactor -= 1;
            }
        }

        public void SetLRBalanceGradually(object step)
        {
            int stepNum = (int)step;
            bool isPositive = stepNum > 0;
            stepNum = Math.Abs(stepNum);
            int count = 0;
            while (count < stepNum && !userInterruptedLR)
            {
                count++;
                Thread.Sleep(waitTimeLR);
                totalFactor.BeginInvoke(new booleanDelegate(PM1LRBalance), new object[] { isPositive });
            }
            LRThread = null;
        }

        public void AsyncSetLRBalance(int balance)
        {
            int stepNum = balance - LRBalance;
            if (stepNum != 0)
                waitTimeLR = (int)Math.Abs(fadeTimeMs / stepNum);
            LRThread = new Thread(new ParameterizedThreadStart(SetLRBalanceGradually));
            LRThread.IsBackground = true;
            LRThread.Start(stepNum);
        }

        public void SetTotalFactorGradually(object step)
        {
            int stepNum = (int)step;
            bool isPositive = stepNum > 0;
            stepNum = Math.Abs(stepNum);
            int count = 0;
            while (count < stepNum && !userInterruptedTF)
            {
                count++;
                totalFactor.BeginInvoke(new booleanDelegate(PM1TotalFactor), new object[] { isPositive });
                Thread.Sleep(waitTimeTF);
            }
            TFThread = null;
        }

        public void AsyncSetTotalFactor(int tfactor)
        {
            int stepNum = tfactor - TotalFactor;

            if (tfactor * TotalFactor < 0)
            {
                // avoid overrunning 0
                //stepNum -= 1;
            }
            if (stepNum != 0)
                waitTimeTF = (int)Math.Abs(fadeTimeMs / stepNum);
            TFThread = new Thread(new ParameterizedThreadStart(SetTotalFactorGradually));
            TFThread.IsBackground = true;
            TFThread.Start(stepNum);
        }

        void playButton_Clicked(object sender, EventArgs e)
        {
            if (playButton.Text == "Play")
            {
                playButton.Text = "Pause";
                Streamable stream = wp.GetCurrentStream();
                if (wd.Equals(stream))
                {
                    wp.Resume();
                }
                else Play();
            }
            else if (playButton.Text == "Pause")
            {
                playButton.Text = "Play";
                wp.Pause();
            }
            titleText.Focus();

        }

        int triple = 0;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        void HandleKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == myKey)
            {
                if (e.Modifiers == Keys.Shift)
                {
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.RShiftKey)))
                        triple++;
                    else triple--;
                    if (triple == 1) lrBalance.Value = lrBalance.Maximum;
                    else if (triple == -1) lrBalance.Value = lrBalance.Minimum;
                    else
                    {
                        lrBalance.Value = (lrBalance.Maximum + lrBalance.Minimum) / 2;
                        triple = 0;
                    }
                }
                else if (e.Modifiers == Keys.Control)
                {
                    int now = totalFactor.Value;
                    int max = totalFactor.Maximum;
                    int min = totalFactor.Minimum;
                    int diff = (max - min) / 10;
                    if (Convert.ToBoolean(GetAsyncKeyState(Keys.RControlKey)))
                    {
                        now += diff;
                        if (now <= max)
                            totalFactor.Value = now;
                        else totalFactor.Value = max;
                    }
                    else
                    {
                        now -= diff;
                        if (now >= min)
                            totalFactor.Value = now;
                        else totalFactor.Value = min;
                    }
                }
                else
                {
                    mute.Checked = !mute.Checked;
                }
            }
            if (CharUtl.CanProcess(e.KeyCode))
            {
                mute.Checked = !CharUtl.SelectIsMute(e.KeyCode, myName);
            }
        }

        void ToggleDPS(object sender, EventArgs e)
        {
            wd.DSPEnabled = DSPEnable.Checked;
        }

        void OffsetChanged(object sender, EventArgs e)
        {
            double offsetms = Convert.ToDouble(offsetUpDown.Value);
            wd.SetOffset(offsetms);
            PlotLR();
        }

        void WheelRotated(object sender, MouseEventArgs e)
        {
            double timePosition = 0;
            try
            {
                timePosition = chart.ChartAreas[0].AxisX.PixelPositionToValue(e.Location.X);
            }
            catch (ArgumentException)
            {
                timePosition = 0;
            }
            
            double ratioPosition = wd.TimeToRatio(timePosition);
            double newspan = 0;
            if (e.Delta < 0)
            {
                newspan = span * 1.1;
            }
            else
            {
                newspan = span * 0.9;
            }
            double x = (ratioPosition-(center-span/2)) / span;
            double newCenter = (ratioPosition-newspan*x)+newspan/2;

            if (newCenter > 1) newCenter = 1;
            else if (newCenter < 0) newCenter = 0;
            if (newspan > 1) { newspan = 1; newCenter = 0.5; }
            else if (newspan < 0) newspan = 0;

            center = newCenter;
            span = newspan;
            PlotLR();
            PlotRefreshed(this, EventArgs.Empty);
        }

        void ChartClicked(object sender, MouseEventArgs e)
        {
            chart.Focus();
        }

        void PaintMask(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            double[] sumfac = ic.GetFactorSum(true);
            double factorL = Math.Abs(wd.GetFactor(0, WavData.LEFT));
            double factorR = Math.Abs(wd.GetFactor(0, WavData.RIGHT));
            
            int alphaL = FactorToAlpha(factorL / sumfac[0]);
            int alphaR = FactorToAlpha(factorR / sumfac[1]);
            if (sumfac[0] < nodivbyzero) alphaL = 230;
            if (sumfac[1] < nodivbyzero) alphaR = 230;

            Color color = Color.FromArgb(alphaL, mainPanelBack);
            Brush b = new SolidBrush(color);
            g.FillRectangle(b, 0, 0, 60, 120);
            color = Color.FromArgb(alphaR, mainPanelBack);
            b = new SolidBrush(color);
            g.FillRectangle(b, 60, 0, 120, 120);

            Font fnt = new System.Drawing.Font("Meiryo UI", 15, FontStyle.Bold); ;
            g.DrawString(myKey.ToString(), fnt, Brushes.Black, 0, 0);
        }

        int FactorToAlpha(double x)
        {
            // (0, 230) , (1, 0) is fixed
            if (x < 0 || x > 1)
            {
                Console.WriteLine(x);
                return 255;
            }
            return (int)(345.24*Math.Pow(x, 4)-1307.1*Math.Pow(x,3)+1748*Math.Pow(x,2)-1016.1*x+230);
        }

        void iconClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                Suicided(this, EventArgs.Empty);
            }
            else if (e.Button == MouseButtons.Right)
            {
                DSPEnable.Checked = !DSPEnable.Checked;
            }
            else
            {
                mute.Checked = !mute.Checked;
            }
        }

        void RequestRefresh()
        {
            if (FactorChanged != null)
                FactorChanged(this, EventArgs.Empty);
        }

        void muteChanged(object sender, EventArgs e)
        {
            wd.Muted = mute.Checked;
            RequestRefresh();
            Refresh();
        }

        void balanceChanged(object sender, EventArgs e)
        {
            double val = lrBalance.Value;
            double factorR = Math.Sin(Math.PI / 40 * val + Math.PI / 4);//0.05 * val + 0.5;
            double factorL = Math.Cos(Math.PI / 40 * val + Math.PI / 4);//-0.05 * val + 0.5;
            wd.SetFactor(factorL, WavData.LEFT);
            wd.SetFactor(factorR, WavData.RIGHT);
            RequestRefresh();
        }

        void factorChanged(object sender, EventArgs e)
        {
            wd.SetFactor(totalFactor.Value * 1.0 / totalFactor.Maximum);
            RequestRefresh();
        }

        void LRBalance_MouseDown(object sender, MouseEventArgs e)
        {
            userInterruptedLR = true;
        }

        void LRBalance_MouseUp(object sender, MouseEventArgs e)
        {
            userInterruptedLR = false;
        }

        void TotalFactor_MouseDown(object sender, MouseEventArgs e)
        {
            userInterruptedTF = true;
        }

        void TotalFactor_MouseUp(object sender, MouseEventArgs e)
        {
            userInterruptedTF = false;
        }
    }
}
