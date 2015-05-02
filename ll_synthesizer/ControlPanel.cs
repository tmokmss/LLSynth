using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ll_synthesizer
{
    class ControlPanel: TableLayoutPanel
    {
        private static Font defaultFont;
        private TrackBar seekBar = new TrackBar();
        private TrackBar volumeBar = new TrackBar();
        private Button playButton = new Button();
        private Button stopButton = new Button();
        private Button backButton = new Button();
        private Label titleLabel = new Label();
        private Label timeLabel = new Label();
        private TableLayoutPanel buttonPanel = new TableLayoutPanel();
        private CheckBox autoDJCheck = new CheckBox();
        private CheckBox repeatCheck = new CheckBox();

        private bool playButtonIsPlay = true;   // if play button is "Play", not "Pause", true
        private int maxTime = 0;

        private static WavPlayer wp;
        private delegate void progressDelegate(int value, int maxTime);
        private delegate void stringDelegate(String text);
        private delegate void generalDelegate();

        public ControlPanel()
        {
            Initialize();
        }

        private void Initialize()
        {
            playButton.Name = "playButton";
            playButton.UseVisualStyleBackColor = true;
            playButton.Text = "Play";
            playButton.AutoSize = true;
            playButton.Font = defaultFont;
            playButton.Click += new System.EventHandler(this.playButton_Click);

            stopButton.Name = "stopButton";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Text = "Stop";
            stopButton.AutoSize = true;
            stopButton.Font = defaultFont;
            stopButton.Click += new System.EventHandler(this.stopButton_Click);

            backButton.Name = "backButton";
            backButton.UseVisualStyleBackColor = true;
            backButton.Text = "Back";
            backButton.AutoSize = true;
            backButton.Font = defaultFont;
            backButton.Click += new System.EventHandler(this.backButton_Click);

            seekBar.Name = "seekBar";
            seekBar.Size = new Size(180, 40);
            seekBar.SmallChange = 1;
            seekBar.TickFrequency = 0;
            seekBar.Minimum = 0;
            seekBar.Maximum = maxTime;
            seekBar.MouseCaptureChanged += new System.EventHandler(this.seekBar_MouseCaptureChanged);
            seekBar.ValueChanged += new System.EventHandler(this.seekBar_ValueChanged);
            seekBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.seekBar_MouseDown);
            seekBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.seekBar_MouseUp);

            volumeBar.Name = "volumeBar";
            volumeBar.Maximum = 0;
            volumeBar.Minimum = -3000;
            volumeBar.Size = new Size(100, 40);
            volumeBar.SmallChange = 50;
            volumeBar.TickFrequency = 0;
            volumeBar.ValueChanged += new System.EventHandler(this.volumeBar_ValueChanged);
            volumeBar.Value = -1000;

            autoDJCheck.Text = "Auto DJ";
            autoDJCheck.AutoSize = true;
            //autoDJCheck.CheckedChanged

            repeatCheck.Text = "Repeat";
            repeatCheck.AutoSize = true;
            repeatCheck.Font = defaultFont;
            repeatCheck.CheckedChanged += this.repeatCheck_CheckedChanged;

            titleLabel.AutoSize = true;
            titleLabel.Font = defaultFont;
            timeLabel.AutoSize = true;
            timeLabel.Font = defaultFont;

            buttonPanel.AutoSize = true;
            buttonPanel.Controls.Add(playButton, 0, 0);
            buttonPanel.Controls.Add(stopButton, 1, 0);
            buttonPanel.Controls.Add(repeatCheck, 2, 0);
            buttonPanel.Controls.Add(backButton, 3, 0);

            this.Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            this.AutoSize = true;
            //this.Padding = 
            this.Name = "controlsPanel";
            this.BackColor = Color.Firebrick;
            this.Controls.Add(titleLabel);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(seekBar);
            this.Controls.Add(volumeBar);
            this.Controls.Add(timeLabel);

            wp.PlayReachedBy += new WavPlayer.ProcessEventHandler(this.ReportReceived);
        }

        public static void SetFont(Font newfont)
        {
            defaultFont = newfont;
        }

        public static void SetWavPlayer(WavPlayer wp) {
            ControlPanel.wp = wp;
        }

        private void ChangePlayButton()
        {
            if (playButtonIsPlay)
            {
                playButton.Text = "Pause";
                playButtonIsPlay = false;
            }
            else
            {
                playButton.Text = "Play";
                playButtonIsPlay = true;
            }
        }

        private void ChangeProgress(int value, int maxTime)
        {
            this.maxTime = maxTime;
            seekBar.Value = value;
            if (maxTime != seekBar.Maximum)
            {
                seekBar.Maximum = maxTime;
            }
            UpdateTimeLabel();
        }

        private void UpdateTimeLabel()
        {
            String timenow = TimeToString(seekBar.Value);
            String timemax = TimeToString(maxTime);
            timeLabel.Text = timenow + " / " + timemax;
        }

        private String TimeToString(int time)
        {
            int min = time / 60;
            int sec = time - min*60;
            return String.Format("{0}:{1:D2}", min, sec);
        }

        private void SetTitle(String title)
        {
            titleLabel.Text = title;
        }

        #region process events
        private void playButton_Click(object sender, EventArgs e)
        {
            bool successful = true;
            if (playButtonIsPlay)
            {
                successful = wp.Resume();
            }
            else
            {
                wp.Pause();
            }
            if (successful)
            {
                ChangePlayButton();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            wp.Stop();
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            wp.Seek(0);
        }

        private void repeatCheck_CheckedChanged(object sender, EventArgs e)
        {
            wp.Repeat = repeatCheck.Checked;
        }

        void ReportReceived(object sender, ProcessEventArgs e)
        {
            double progress = e.progress;
            int value = (int)Math.Round((seekBar.Maximum * e.progress));
            if (value > seekBar.Maximum) value = seekBar.Maximum;
            seekBar.BeginInvoke(new progressDelegate(ChangeProgress), new object[] { value, e.maxTimeSeconds });
            if (value == seekBar.Maximum && !repeatCheck.Checked)
            {
                playButton.BeginInvoke(new generalDelegate(ChangePlayButton));
            }
            if (value < seekBar.Maximum && playButtonIsPlay)
            {
                // if sound played but playButton is not "Pause"
                playButton.BeginInvoke(new generalDelegate(ChangePlayButton));
            }
            if (titleLabel.Text != e.title)
            {
                titleLabel.BeginInvoke(new stringDelegate(SetTitle), new object[] {e.title});
            }
        }

        private void seekBar_MouseCaptureChanged(object sender, EventArgs e)
        {
            double ratio = seekBar.Value * 1.0 / seekBar.Maximum;
            wp.Seek(ratio);
        }

        private void seekBar_ValueChanged(object sender, EventArgs e)
        {
            UpdateTimeLabel();
        }

        private void seekBar_MouseDown(object sender, MouseEventArgs e)
        {
            wp.PlayReachedBy -= new WavPlayer.ProcessEventHandler(this.ReportReceived);
        }

        private void seekBar_MouseUp(object sender, MouseEventArgs e)
        {
            wp.PlayReachedBy += new WavPlayer.ProcessEventHandler(this.ReportReceived);
        }

        private void volumeBar_ValueChanged(object sender, EventArgs e)
        {
            wp.Volume = volumeBar.Value;
        }
        #endregion
    }
}
