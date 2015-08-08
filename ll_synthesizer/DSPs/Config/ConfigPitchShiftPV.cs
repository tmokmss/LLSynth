using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ll_synthesizer.DSPs.Types;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigPitchShiftPV : ConfigWindow
    {
        TrackBar shiftRateBar= new TrackBar();
        PitchShiftPV myDSP;

        int fac = 100;

        public ConfigPitchShiftPV(DSP dsp): base(dsp)
        {
            myDSP = (PitchShiftPV) dsp;
            Initialize();
        }

        private void Initialize()
        {
            shiftRateBar.Maximum = 2*fac;
            shiftRateBar.Minimum = 0;

            shiftRateBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(shiftRateBar);
        }

        private void upDownChanged(object sender, EventArgs e)
        {
            myDSP.ShiftRate = (shiftRateBar.Value - shiftRateBar.Minimum) * 2.0/ shiftRateBar.Maximum;
        }

    }
}
