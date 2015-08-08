using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Types;

namespace ll_synthesizer.DSPs
{
    class DSPSelector
    {
        DSP myDSP = new Default();
        DSPType currentType = DSPType.Default;

        public DSPType CurrentType
        {
            set { ChangeDSP(value); }
            get { return currentType; }
        }

        public void Process(ref short[] left, ref short[] right)
        {
            myDSP.Process(ref left, ref right);
        }

        public void ShowConfigWindow(string title)
        {
            myDSP.ShowConfigWindow(title);
        }

        private void ChangeDSP(DSPType type)
        {
            if (type == currentType) return;
            myDSP.Dispose();
            switch(type)
            {
                case DSPType.CenterCut:
                    myDSP = new CenterCut();
                    break;
                case DSPType.PitchShiftPV:
                    myDSP = new PitchShiftPV();
                    break;
                case DSPType.PitchShiftTDSOLA:
                    myDSP = new PitchShiftTDSOLA();
                    break;
                default:
                    myDSP = new Default();
                    break;
            }
            currentType = type;
        }
    }
}
