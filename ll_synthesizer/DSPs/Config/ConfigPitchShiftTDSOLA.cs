using System;
using ll_synthesizer.DSPs.Types;
using System.Windows.Forms;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigPitchShiftTDSOLA : ConfigWindow
    {
        TrackBar shiftRateBar = new TrackBar();
        PitchShiftTDSOLA myDSP;

        int fac = 100;

        public ConfigPitchShiftTDSOLA(DSP dsp) : base(dsp)
            {
            myDSP = (PitchShiftTDSOLA)dsp;
            Initialize();
        }

        private void Initialize()
        {
            shiftRateBar.Maximum = 2 * fac;
            shiftRateBar.Minimum = 0;

            var nowRate = myDSP.ShiftRate;
            var nowValue = (int)(nowRate * shiftRateBar.Maximum / 2 + shiftRateBar.Minimum);
            shiftRateBar.Value = nowValue;

            shiftRateBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(shiftRateBar);
        }

        private void upDownChanged(object sender, EventArgs e)
        {
            var newRate = (shiftRateBar.Value - shiftRateBar.Minimum) * 2.0 / shiftRateBar.Maximum;
            myDSP.ShiftRate = newRate;
            Console.WriteLine(newRate);
        }
    }
}
