using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer
{
    interface Streamable
    {
        void GetLRBuffer(int start, int size, out short[] left, out short[] right);
        int GetLength();
        int GetMaxTimeSeconds();
        String GetTitle();
    }
}
