using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ll_synthesizer.DSPs.Types;


namespace ll_synthesizer.DSPs.Config
{
    class ConfigHighPassFilter : ConfigWindow
    {
        TrackBar freqBar= new TrackBar();
        HighPassFilter myDSP;

        int fac = 100;

        public ConfigHighPassFilter(DSP dsp)
            : base(dsp)
        {
            myDSP = (HighPassFilter)dsp;
            Initialize();
        }

        private void Initialize()
        {
            freqBar.Maximum = 2000;
            freqBar.Minimum = 0;

            var nowValue = (int)myDSP.CutoffFrequency;
            freqBar.Value = nowValue;

            freqBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(freqBar);
        }

        private void upDownChanged(object sender, EventArgs e)
        {
            myDSP.CutoffFrequency = freqBar.Value;
        }

    }
}
