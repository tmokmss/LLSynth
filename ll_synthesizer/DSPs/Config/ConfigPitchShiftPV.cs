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
        private static readonly double ONEDEG = Math.Pow(2, 1.0 / 12);
        TrackBar shiftRateBar= new TrackBar();
        PitchShiftPV myDSP;

        int fac = 10;

        public ConfigPitchShiftPV(DSP dsp): base(dsp)
        {
            myDSP = (PitchShiftPV) dsp;
            Initialize();
        }

        private void Initialize()
        {
            shiftRateBar.Maximum = 2*fac;
            shiftRateBar.Minimum = -2*fac;

            var nowRate = myDSP.ShiftRate;
            var nowValue = (int)(nowRate * shiftRateBar.Maximum / 2 + shiftRateBar.Minimum);
            shiftRateBar.Value = nowValue;

            shiftRateBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(shiftRateBar);
        }

        private void upDownChanged(object sender, EventArgs e)
        {
            //var newval = (shiftRateBar.Value - shiftRateBar.Minimum) * 2.0 / shiftRateBar.Maximum;
            var newval = Math.Pow(ONEDEG, shiftRateBar.Value);
            myDSP.ShiftRate = newval;
            Console.WriteLine(newval);
        }

    }
}
