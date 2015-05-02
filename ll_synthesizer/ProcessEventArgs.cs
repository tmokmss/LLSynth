using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer
{
    class ProcessEventArgs: EventArgs
    {
        public double progress;
        public int maxTimeSeconds;
        public String title;
        //public bool enable = true;
    }
}