using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigWindow : Form
    {
        DSP myDSP;

        public ConfigWindow(DSP parent)
        {
            myDSP = parent;
        }
    }
}
