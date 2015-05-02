using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ll_synthesizer.Sound
{
    interface SoundFileReader
    {
        int Position
        {
            set;
            get;
        }
        int Length
        {
            get;
        }
        void Read(byte[] buffer, int offset, int count);
        void Dispose();
    }
}
