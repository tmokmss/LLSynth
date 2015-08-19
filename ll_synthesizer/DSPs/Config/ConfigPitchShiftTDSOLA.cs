using System;
using ll_synthesizer.DSPs.Types;
using System.Windows.Forms;
using System.Drawing;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigPitchShiftTDSOLA : ConfigWindow
    {
        TrackBar fShiftRateBar = new TrackBar();
        TrackBar pShiftRateBar = new TrackBar();
        PitchShiftTDSOLA myDSP;

        int fac = 100;

        public ConfigPitchShiftTDSOLA(DSP dsp) : base(dsp)
            {
            myDSP = (PitchShiftTDSOLA)dsp;
            Initialize();
        }

        private void Initialize()
        {
            fShiftRateBar.Maximum = 2 * fac;
            fShiftRateBar.Minimum = 0;

            var nowRate = myDSP.FormantShiftRate;
            var nowValue = (int)(nowRate * fShiftRateBar.Maximum / 2 + fShiftRateBar.Minimum);
            fShiftRateBar.Value = nowValue;

            fShiftRateBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(fShiftRateBar);


            pShiftRateBar.Maximum = 2 * fac;
            pShiftRateBar.Minimum = 0;
            nowRate = myDSP.PitchShiftRate;
            nowValue = (int)(nowRate * pShiftRateBar.Maximum / 2 + pShiftRateBar.Minimum);
            pShiftRateBar.Value = nowValue;
            pShiftRateBar.Location = new Point(10, 100);
            pShiftRateBar.ValueChanged += new System.EventHandler(this.upDownChanged);
            this.Controls.Add(pShiftRateBar);
        }

        private void upDownChanged(object sender, EventArgs e)
        {
            var newRate = (fShiftRateBar.Value - fShiftRateBar.Minimum) * 2.0 / fShiftRateBar.Maximum;
            myDSP.FormantShiftRate = newRate;
            Console.WriteLine(newRate);
            newRate = (pShiftRateBar.Value - pShiftRateBar.Minimum) * 2.0 / pShiftRateBar.Maximum;
            myDSP.PitchShiftRate = newRate;
            Console.WriteLine(newRate);
        }
    }
}
