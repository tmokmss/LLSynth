namespace ll_synthesizer
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.chartPanel = new System.Windows.Forms.Panel();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.polarButton = new System.Windows.Forms.Button();
            this.tbar2ResetButton = new System.Windows.Forms.Button();
            this.saveCheck = new System.Windows.Forms.CheckBox();
            this.fadeTimeBar = new System.Windows.Forms.TrackBar();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.lrButton = new System.Windows.Forms.Button();
            this.vocalRstr = new System.Windows.Forms.Label();
            this.vocalLstr = new System.Windows.Forms.Label();
            this.pauseButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.autoCheck = new System.Windows.Forms.CheckBox();
            this.offsetUpDown = new System.Windows.Forms.NumericUpDown();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowChartPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.baseTablePanel = new System.Windows.Forms.TableLayoutPanel();
            this.chartPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeTimeBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsetUpDown)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.baseTablePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // chartPanel
            // 
            resources.ApplyResources(this.chartPanel, "chartPanel");
            this.chartPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.chartPanel.Controls.Add(this.trackBar2);
            this.chartPanel.Controls.Add(this.polarButton);
            this.chartPanel.Controls.Add(this.tbar2ResetButton);
            this.chartPanel.Controls.Add(this.saveCheck);
            this.chartPanel.Controls.Add(this.fadeTimeBar);
            this.chartPanel.Controls.Add(this.trackBar1);
            this.chartPanel.Controls.Add(this.lrButton);
            this.chartPanel.Controls.Add(this.vocalRstr);
            this.chartPanel.Controls.Add(this.vocalLstr);
            this.chartPanel.Controls.Add(this.pauseButton);
            this.chartPanel.Controls.Add(this.refreshButton);
            this.chartPanel.Controls.Add(this.autoCheck);
            this.chartPanel.Controls.Add(this.offsetUpDown);
            this.chartPanel.Controls.Add(this.button3);
            this.chartPanel.Controls.Add(this.button4);
            this.chartPanel.Controls.Add(this.button1);
            this.chartPanel.Name = "chartPanel";
            // 
            // trackBar2
            // 
            resources.ApplyResources(this.trackBar2, "trackBar2");
            this.trackBar2.Maximum = 2000;
            this.trackBar2.Minimum = 10;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.SmallChange = 10;
            this.trackBar2.TickFrequency = 100;
            this.trackBar2.Value = 10;
            this.trackBar2.Scroll += new System.EventHandler(this.trackBar2_Scroll);
            // 
            // polarButton
            // 
            resources.ApplyResources(this.polarButton, "polarButton");
            this.polarButton.Name = "polarButton";
            this.polarButton.UseVisualStyleBackColor = true;
            this.polarButton.Click += new System.EventHandler(this.polarButton_Click);
            // 
            // tbar2ResetButton
            // 
            resources.ApplyResources(this.tbar2ResetButton, "tbar2ResetButton");
            this.tbar2ResetButton.Name = "tbar2ResetButton";
            this.tbar2ResetButton.UseVisualStyleBackColor = true;
            this.tbar2ResetButton.Click += new System.EventHandler(this.tbar2ResetButton_Click);
            // 
            // saveCheck
            // 
            resources.ApplyResources(this.saveCheck, "saveCheck");
            this.saveCheck.Image = global::ll_synthesizer.Properties.Resources.control_record;
            this.saveCheck.Name = "saveCheck";
            this.saveCheck.UseVisualStyleBackColor = false;
            this.saveCheck.CheckedChanged += new System.EventHandler(this.saveCheck_CheckedChanged);
            // 
            // fadeTimeBar
            // 
            resources.ApplyResources(this.fadeTimeBar, "fadeTimeBar");
            this.fadeTimeBar.Maximum = 2000;
            this.fadeTimeBar.Name = "fadeTimeBar";
            this.fadeTimeBar.SmallChange = 100;
            this.fadeTimeBar.TickFrequency = 0;
            this.fadeTimeBar.MouseCaptureChanged += new System.EventHandler(this.fadeTimeBar_MouseCaptureChanged);
            // 
            // trackBar1
            // 
            resources.ApplyResources(this.trackBar1, "trackBar1");
            this.trackBar1.Maximum = 100;
            this.trackBar1.Minimum = -100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.Value = 20;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // lrButton
            // 
            resources.ApplyResources(this.lrButton, "lrButton");
            this.lrButton.Name = "lrButton";
            this.lrButton.UseVisualStyleBackColor = true;
            this.lrButton.Click += new System.EventHandler(this.lrButton_Click);
            // 
            // vocalRstr
            // 
            resources.ApplyResources(this.vocalRstr, "vocalRstr");
            this.vocalRstr.Name = "vocalRstr";
            // 
            // vocalLstr
            // 
            resources.ApplyResources(this.vocalLstr, "vocalLstr");
            this.vocalLstr.Name = "vocalLstr";
            // 
            // pauseButton
            // 
            resources.ApplyResources(this.pauseButton, "pauseButton");
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.UseVisualStyleBackColor = true;
            this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // refreshButton
            // 
            resources.ApplyResources(this.refreshButton, "refreshButton");
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // autoCheck
            // 
            resources.ApplyResources(this.autoCheck, "autoCheck");
            this.autoCheck.Name = "autoCheck";
            this.autoCheck.UseVisualStyleBackColor = true;
            this.autoCheck.CheckedChanged += new System.EventHandler(this.autoCheck_CheckedChanged);
            // 
            // offsetUpDown
            // 
            this.offsetUpDown.DecimalPlaces = 2;
            this.offsetUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            resources.ApplyResources(this.offsetUpDown, "offsetUpDown");
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
            this.offsetUpDown.Name = "offsetUpDown";
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            resources.ApplyResources(this.button4, "button4");
            this.button4.Name = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openFileToolStripMenuItem,
            this.addFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openDirectoryToolStripMenuItem_Click);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            resources.ApplyResources(this.openFileToolStripMenuItem, "openFileToolStripMenuItem");
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // addFileToolStripMenuItem
            // 
            this.addFileToolStripMenuItem.Name = "addFileToolStripMenuItem";
            resources.ApplyResources(this.addFileToolStripMenuItem, "addFileToolStripMenuItem");
            this.addFileToolStripMenuItem.Click += new System.EventHandler(this.addFileToolStripMenuItem_Click);
            // 
            // flowChartPanel
            // 
            resources.ApplyResources(this.flowChartPanel, "flowChartPanel");
            this.flowChartPanel.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.baseTablePanel.SetColumnSpan(this.flowChartPanel, 2);
            this.flowChartPanel.Name = "flowChartPanel";
            this.flowChartPanel.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.flowChartPanel_ControlRemoved);
            this.flowChartPanel.Enter += new System.EventHandler(this.flowChartPanel_Enter);
            this.flowChartPanel.Leave += new System.EventHandler(this.flowChartPanel_Leave);
            this.flowChartPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.flowChartPanel_MouseDown);
            // 
            // baseTablePanel
            // 
            resources.ApplyResources(this.baseTablePanel, "baseTablePanel");
            this.baseTablePanel.Controls.Add(this.chartPanel, 0, 1);
            this.baseTablePanel.Controls.Add(this.flowChartPanel, 0, 0);
            this.baseTablePanel.Name = "baseTablePanel";
            // 
            // Form1
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.baseTablePanel);
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.chartPanel.ResumeLayout(false);
            this.chartPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fadeTimeBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsetUpDown)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.baseTablePanel.ResumeLayout(false);
            this.baseTablePanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel chartPanel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowChartPanel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.NumericUpDown offsetUpDown;
        private System.Windows.Forms.CheckBox autoCheck;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button pauseButton;
        private System.Windows.Forms.Label vocalRstr;
        private System.Windows.Forms.Label vocalLstr;
        private System.Windows.Forms.Button lrButton;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.TrackBar fadeTimeBar;
        private System.Windows.Forms.TableLayoutPanel baseTablePanel;
        private System.Windows.Forms.CheckBox saveCheck;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFileToolStripMenuItem;
        private System.Windows.Forms.Button tbar2ResetButton;
        private System.Windows.Forms.Button polarButton;
        private System.Windows.Forms.TrackBar trackBar2;
    }
}

