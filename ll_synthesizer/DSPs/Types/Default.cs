using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.DSPs.Types
{
    class Default : DSP
    {
        public override DSPType Type
        {
            get { return DSPType.Default; }
        }

        public override void Process(ref short[] left, ref short[] right)
        {
            return;
        }
    }
}
