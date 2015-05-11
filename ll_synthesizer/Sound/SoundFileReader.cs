using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

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
        WaveFormat WaveFormat {get;}
    }
}
