using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Types;

namespace ll_synthesizer.DSPs
{
    class DSPSelector
    {
        private DSP myDSP = new Default();
        private DSPType currentType = DSPType.Default;

        public DSPType CurrentType
        {
            set { ChangeDSP(value); }
            get { return currentType; }
        }

        public void Dispose()
        {
            myDSP.Dispose();
            myDSP = null;
        }

        public DSP GetDSP()
        {
            return myDSP;
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
                case DSPType.HighPassFilter:
                    myDSP = new HighPassFilter();
                    break;
                case DSPType.BandPassFilter:
                    myDSP = new BandPassFilter();
                    break;
                case DSPType.Butterworth1stLPF:
                    myDSP = new Butterworth1stLPF();
                    break;
                default:
                    myDSP = new Default();
                    break;
            }
            currentType = type;
        }
    }
}
