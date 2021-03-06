﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ll_synthesizer.DSPs.Types;

namespace ll_synthesizer.DSPs.Config
{
    class ConfigWindowFactory
    {
        private static ConfigWindowFactory instance = new ConfigWindowFactory();
        private ConfigWindowFactory() { }

        public static ConfigWindowFactory GetInstance()
        {
            return instance;
        }

        public ConfigWindow CreateConfigWindow(DSP parent)
        {
            switch(parent.Type)
            {
                case DSPType.PitchShiftPV:
                    return new ConfigPitchShiftPV((PitchShiftPV)parent);
                case DSPType.PitchShiftTDSOLA:
                    return new ConfigPitchShiftTDSOLA((PitchShiftTDSOLA)parent);
                case DSPType.HighPassFilter:
                    return new ConfigHighPassFilter((HighPassFilter)parent);
                case DSPType.BandPassFilter:
                    return new ConfigBandPassFilter((BandPassFilter)parent);
                case DSPType.Butterworth1stLPF:
                    return new ConfigButterworth1stLPF((Butterworth1stLPF)parent);
                default:
                    return new ConfigWindow(parent);
            }
        }

    }
}
