using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ll_synthesizer.DSPs.Types;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigButterworth1stLPF : ConfigWindow
    {
        TrackBar freqBar = new TrackBar();
        Butterworth1stLPF myDSP;

        int fac = 100;

        public ConfigButterworth1stLPF(DSP dsp)
            : base(dsp)
        {
            myDSP = (Butterworth1stLPF)dsp;
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
