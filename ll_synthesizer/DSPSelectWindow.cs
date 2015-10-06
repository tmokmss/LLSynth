using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ll_synthesizer.DSPs;

namespace ll_synthesizer
{
    class DSPSelectWindow : Form
    {
        WavData myWd;
        ComboBox dspList = new ComboBox();

        public DSPSelectWindow(WavData parent)
        {
            myWd = parent;
            Initialize();
        }

        private void Initialize()
        {
            dspList.SelectedIndexChanged += new System.EventHandler(this.SelectDsp);

            ResetDspList();

            this.Controls.Add(dspList);
        }

        private void ResetDspList()
        {
            foreach (DSPType type in Enum.GetValues(typeof(DSPType)))
            {
                dspList.Items.Add(type.ToString());
            }
        }

        private void SelectDsp(object sender, EventArgs e)
        {
            DSPType type = (DSPType)Enum.Parse(typeof(DSPType), dspList.Text);
            myWd.SetCurrentDSP(type);
        }

    }
}
