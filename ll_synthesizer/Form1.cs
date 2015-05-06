using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ll_synthesizer.DSPs;

namespace ll_synthesizer
{
    public partial class Form1 : Form
    {
        private static String appName = "LoveLive Synthesizer";
        private String folderPath = "";
        private static Font defaultFont = new Font("Meiryo UI", 9);
        private FileGetter fg;
        private WavPlayer wp;
        private ItemCombiner ic;
        private ControlPanel cp;

        private int randomizeInterval = 3;
        
        delegate void progressDelegate(int value);
        delegate void generalDelegate();

        public Form1()
        {
            InitializeComponent();
            init();
            refresh();
        }

        void init()
        {
            Settings settings = Settings.GetInstance();
            settings.SaveSettings();
            wp = new WavPlayer(this);
            ItemCombiner.SetWavPlayer(wp);
            GraphPanel.SetFont(defaultFont);
            ItemSet.SetWavPlayer(wp);
            ItemSet.SetForm(this);
            wp.PlayReachedBy += new WavPlayer.ProcessEventHandler(this.ReportReceived);
            this.KeyPreview = true;
            ControlPanel.SetFont(defaultFont);
            ControlPanel.SetWavPlayer(wp);
            cp = new ControlPanel();
            //cp.Location = new Point(500, 560);
            //this.Controls.Add(cp);
            baseTablePanel.Controls.Add(cp, 1, 1);
            FHTransform.Initialize(WavData.BufSizeDefault);
        }

        void debug()
        {
            folderPath = @"H:\jump";
            LoadFiles();
        }

        private void refresh()
        {
            wp.Stop();
            this.Text = appName + " " + folderPath;
            flowChartPanel.Controls.Clear();
            if (ic != null)
            {
                ic.Dispose();
                ic = null;
            }
            ic = new ItemCombiner(this);
            ItemSet.SetCombiner(ic);
        }

        void AddItem(String file)
        {
            ic.AddItem(new ItemSet(file));
            AddChart(ic.GetLastItem());
        }

        public void AddItems(String[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                AddItem(files[i]);
            }
        }

        private void AddChart(ItemSet item) {
            this.flowChartPanel.Controls.Add(item);
        }

        void LoadFiles()
        {
            refresh();
            fg = new FileGetter(folderPath);
            String[] files = fg.GetList();
            if (files.Length > 0)
            {
                AddItems(fg.GetList());
                //ic.AdjustOffset();
                ic.AsyncAdjustOffset();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.flowChartPanel.BackColor = Color.AntiqueWhite;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select folder that includes wav|mp3 files:";
            if (fbd.ShowDialog(this) == DialogResult.OK) {
                folderPath = fbd.SelectedPath;
                LoadFiles();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            debug();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            wp.Stop();
            button3.Enabled = true;
        }

        private void flowChartPanel_Enter(object sender, EventArgs e)
        {
           // flowChartPanel.VerticalScroll.Enabled = true;
        }

        private void flowChartPanel_Leave(object sender, EventArgs e)
        {
            //flowChartPanel.VerticalScroll.Enabled = false;
        }

        bool scroll = false;

        private void flowChartPanel_MouseDown(object sender, MouseEventArgs e)
        {
            scroll = !scroll;
            flowChartPanel.VerticalScroll.Enabled = scroll;
            //flowChartPanel.Focus();
        }

        int count = 0;
        void ReportReceived(object sender, ProcessEventArgs e)
        {
            if (++count >= randomizeInterval)
            {
                if (autoCheck.Checked)
                {
                    this.BeginInvoke(new generalDelegate(ic.ApplyRandomizedFactor));
                }
                count = 0;
            }
        }

        void SetVocalStrength(String[] str)
        {
            vocalLstr.Text = str[0];
            vocalRstr.Text = str[1];
        }

        public event KeyEventHandler KeyPushed;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (KeyPushed != null)
                KeyPushed(this, e);
        }

        private void flowChartPanel_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (flowChartPanel.Controls.Count == 0)
            {
                refresh();
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            if (wp.IsPlaying())
            {
                wp.Pause();
                pauseButton.Text = "Resume";
            }
            else
            {
                wp.Resume();
                pauseButton.Text = "Pause";
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            wp.Pause();
            foreach (var filePath in (string[])e.Data.GetData(DataFormats.FileDrop))
            {
                if (FileGetter.IsValidFile(filePath))
                    AddItem(filePath);
            }
            ic.AsyncAdjustOffset();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void lrButton_Click(object sender, EventArgs e)
        {
            String[] str = ic.GetLRStrength();
            SetVocalStrength(str);
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            ic.MelodyRemovalRatio = trackBar1.Value * 1.0 / trackBar1.Maximum;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            wp.Play(ic);
        }

        private void fadeTimeBar_MouseCaptureChanged(object sender, EventArgs e)
        {
            ItemSet.FadingTime = fadeTimeBar.Value;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            wp.Close();
            ic.Dispose();
        }
    }
}
