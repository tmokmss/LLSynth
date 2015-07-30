using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace ll_synthesizer
{
    class GraphPanel: TableLayoutPanel
    {
        protected Chart chart = new Chart();
        private ChartArea chartArea = new ChartArea();
        private Legend legend = new Legend();
        private TableLayoutPanel infoPanel = new TableLayoutPanel();
        protected TextBox titleText = new TextBox();
        protected Button playButton = new Button();
        protected NumericUpDown offsetUpDown = new NumericUpDown();
        protected PictureBox icon = new PictureBox();
        protected CheckBox mute = new CheckBox();
        protected CheckBox DSPEnable = new CheckBox();
        protected TrackBar lrBalance = new TrackBar();
        protected TrackBar totalFactor = new TrackBar();
        protected Button dspConfigButton = new Button();

        protected static Font defaultFont;
        private static Color chartActive = Color.DarkCyan;
        private static Color chartInactive = Color.LightCyan;
        protected Color mainPanelBack = Color.AliceBlue;
        private static Bitmap muteImage = new Bitmap("..\\ico\\mute.png");
        private static Bitmap gearImage = new Bitmap("..\\ico\\gear.png");
        protected bool plotEnable = false;
        protected int factorDefault = 5;
        protected string title;
        protected string myName;
        protected string myNum;
        protected Keys myKey;
        private static int sequential = 0;

        public GraphPanel(string title)
        {
            this.title = title;
            this.myNum = (sequential++).ToString();
            InitComponents();
            myKey = ChooseKey();
        }

        public bool PlotEnable
        {
            set
            {
                this.plotEnable = value;
                if (value)
                    this.Controls.Add(chart);
                else
                    this.Controls.Remove(chart);
            }
            get { return plotEnable; }
        }

        public static void SetFont(Font newfont)
        {
            defaultFont = newfont;
        }

        void InitComponents()
        {
            // chart
            chart.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                            | AnchorStyles.Right)));
            chartArea.Name = "ChartArea" + myNum;
            chart.ChartAreas.Add(chartArea);
            legend.Name = "Legend" + myNum;
            chart.Legends.Add(legend);
            chart.Name = "chart" + myNum;
            chart.Palette = ChartColorPalette.Fire;
            chart.AutoSize = true;
            chart.Text = "chart";
            chart.Padding = new System.Windows.Forms.Padding(50);
            chart.Size = new System.Drawing.Size(720, 124);
            chart.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            chart.BackColor = chartInactive;
            chart.Enter += new System.EventHandler(this.chart_Enter);
            chart.Leave += new System.EventHandler(this.chart_Leave);

            // button
            playButton.Name = "playButton" + myNum;
            playButton.Size = new System.Drawing.Size(75, 23);
            playButton.TabIndex = 2;
            playButton.Text = "Play";
            playButton.Font = defaultFont;
            playButton.UseVisualStyleBackColor = true;

            // offsetUpDown
            offsetUpDown.DecimalPlaces = 2;
            this.offsetUpDown.DecimalPlaces = 2;
            this.offsetUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.offsetUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.offsetUpDown.Minimum = new decimal(new int[] {
            1000,
            0,
            0,
            -2147483648});
            offsetUpDown.Name = "offsetUpDown";
            offsetUpDown.Size = new System.Drawing.Size(80, 19);
            offsetUpDown.Font = defaultFont;

            // titleText
            titleText.ReadOnly = true;
            titleText.Name = "titleText";
            titleText.Size = new System.Drawing.Size(180, 19);
            titleText.Text = title;
            titleText.Font = defaultFont;

            // picture
            icon.ImageLocation = ChooseIcon(title);
            icon.Name = "icon" + myNum;
            icon.TabStop = false;
            icon.Width = 120;
            icon.Height = 120;

            // mute
            mute.AutoSize = true;
            mute.Name = "mute" + myNum;
            //mute.Text = "Mute";
            mute.UseVisualStyleBackColor = true;
            mute.Font = defaultFont;
            mute.Image = muteImage;
            mute.Padding = new Padding(0, 3, 8, 0);

            // DSPEnable
            DSPEnable.AutoSize = true;
            DSPEnable.Name = "DSPEnable" + myNum;
            DSPEnable.Text = "DSP";
            DSPEnable.UseVisualStyleBackColor = true;
            DSPEnable.Font = defaultFont;
            
            // DSPConfigButton
            dspConfigButton.Name = "dspConfigButton" + myNum;
            dspConfigButton.Size = new System.Drawing.Size(23, 23);
            dspConfigButton.Font = defaultFont;
            dspConfigButton.Image = gearImage;
            dspConfigButton.UseVisualStyleBackColor = true;

            // LRBalance
            lrBalance.Maximum = 10;
            lrBalance.Minimum = -10;
            lrBalance.Name = "lrBalance" + myNum;
            lrBalance.Size = new System.Drawing.Size(90, 40);
            lrBalance.SmallChange = 1;

            // totalFactor
            totalFactor.Maximum = 10;
            totalFactor.Minimum = -10;
            totalFactor.Name = "totalFactor" + myNum;
            totalFactor.Size = new System.Drawing.Size(90, 40);
            totalFactor.SmallChange = 1;

            // infoPanel
            infoPanel.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                            | AnchorStyles.Right)));
            infoPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            infoPanel.ColumnCount = 3;
            infoPanel.Name = "infoPanel" + myNum;
            infoPanel.AutoSize = true;
            infoPanel.BackColor = Color.AliceBlue;

            infoPanel.Controls.Add(playButton, 0, 0);
            infoPanel.Controls.Add(offsetUpDown, 1, 0);
            infoPanel.Controls.Add(titleText, 0, 1);
            infoPanel.SetColumnSpan(titleText, 2);
            infoPanel.Controls.Add(mute, 0, 2);
            infoPanel.Controls.Add(DSPEnable, 1, 2);
            infoPanel.Controls.Add(dspConfigButton, 2, 2);
            infoPanel.Controls.Add(lrBalance, 0, 3);
            infoPanel.Controls.Add(totalFactor, 1, 3);

            // mainpanel
            this.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                            | AnchorStyles.Right)));
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            this.Name = "chartPanel" + myNum;
            this.AutoSize = true;
            this.BackColor = mainPanelBack;
            this.ColumnCount = 4;

            this.Controls.Add(infoPanel);
            //this.Controls.Add(offsetUpDown);

            if (plotEnable)
                this.Controls.Add(chart);
            if (icon.ImageLocation != null) this.Controls.Add(icon);
        }


        static public void ResetCounter()
        {
            sequential = 0;
        }

        Keys ChooseKey()
        {
            switch(Int32.Parse(myNum)) {
                case 0: return Keys.A;
                case 1: return Keys.S;
                case 2: return Keys.D;
                case 3: return Keys.F;
                case 4: return Keys.G;
                case 5: return Keys.H;
                case 6: return Keys.J;
                case 7: return Keys.K;
                case 8: return Keys.L;
            }
            return Keys.None;
        }

        string ChooseIcon(string title)
        {
            string path;
            if (title.Contains("ELI"))
            {
                myName = "ELI";
                path = "..\\ico\\eli.png";
            }
            else if (title.Contains("HANAYO"))
            {
                myName = "HANAYO";
                path = "..\\ico\\hanayo.png";
            }
            else if (title.Contains("HONOKA"))
            {
                myName = "HONOKA";
                path = "..\\ico\\honoka.png";
            }
            else if (title.Contains("KOTORI"))
            {
                myName = "KOTORI";
                path = "..\\ico\\kotori.png";
            }
            else if (title.Contains("MAKI"))
            {
                myName = "MAKI";
                path = "..\\ico\\maki.png";
            }
            else if (title.Contains("NICO"))
            {
                myName = "NICO";
                path = "..\\ico\\nico.png";
            }
            else if (title.Contains("NOZOMI"))
            {
                myName = "NOZOMI";
                path = "..\\ico\\nozomi.png";
            }
            else if (title.Contains("RIN"))
            {
                myName = "RIN";
                path = "..\\ico\\rin.png";
            }
            else if (title.Contains("UMI"))
            {
                myName = "UMI";
                path = "..\\ico\\umi.png";
            }
            else
            {
                path = "..\\ico\\default.png";
            }
            string basedir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basedir, path);
        }

        public void AddLR(WavData wd, int sampleNum, double start, double end)
        {
            double startTime = wd.RatioToTime(start);
            double endTime = wd.RatioToTime(end);
            double dt = (endTime - startTime) / sampleNum;
            short[] left = wd.GetLeft(sampleNum, start, end);
            short[] right = wd.GetRight(sampleNum, start, end);
            chart.Series.Clear();
            chart.Series.Add("L");
            chart.Series["L"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            for (int i = 0; i < left.Length; i++)
            {
                double x = startTime + dt * i;
                chart.Series["L"].Points.AddXY(x, left[i]);
            }
            chart.Series.Add("R");
            chart.Series["R"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            for (int i = 0; i < right.Length; i++)
            {
                double x = startTime + dt * i;
                chart.Series["R"].Points.AddXY(x, right[i]);
            }
            setXlim(startTime, endTime);
            setYlim(-Math.Pow(2, 15)+1, Math.Pow(2,15) - 1);
        }

        public void setXlim(double min, double max)
        {
            chartArea.AxisX.Minimum = min;
            chartArea.AxisX.Maximum = max;
        }

        public void setYlim(double min, double max)
        {
            chartArea.AxisY.Minimum = min;
            chartArea.AxisY.Maximum = max;
        }

        void chart_Enter(object sender, EventArgs e)
        {
            chart.BackColor = chartActive;
        }

        void chart_Leave(object sender, EventArgs e)
        {
            chart.BackColor = chartInactive;
        }
    }
}
