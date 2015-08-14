using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using ll_synthesizer.DSPs.Types;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigBandPassFilter : ConfigWindow
    {
        TrackBar freqDownBar = new TrackBar();
        TrackBar freqUpBar = new TrackBar();
        BandPassFilter myDSP;

        int fac = 100;

        public ConfigBandPassFilter(DSP dsp)
            : base(dsp)
        {
            myDSP = (BandPassFilter)dsp;
            Initialize();
        }

        private void Initialize()
        {
            freqDownBar.Maximum = 2000;
            freqDownBar.Minimum = 0;
            freqDownBar.Value = (int)myDSP.CutoffFrequencyDown;
            freqDownBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            freqDownBar.Location = new Point(10, 10);
            this.Controls.Add(freqDownBar);

            freqUpBar.Maximum = 44100;
            freqUpBar.Minimum = 0;
            freqUpBar.Value = (int)myDSP.CutoffFrequencyUp;
            freqUpBar.Location = new Point(10, 100);
            freqUpBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(freqUpBar);
        }

        private void upDownChanged(object sender, EventArgs e)
        {
            myDSP.CutoffFrequencyDown = freqDownBar.Value;
            myDSP.CutoffFrequencyUp = freqUpBar.Value;
        }
}

}
