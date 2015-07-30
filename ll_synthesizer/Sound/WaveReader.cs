using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace ll_synthesizer.Sound
{
    class WaveReader: SoundFileReader
    {
        WaveFileReader wfr;
        public WaveReader(string path)
        {
            wfr = new WaveFileReader(path);
        }

        public int Position
        {
            set { wfr.Position = value; }
            get { return (int)wfr.Position; }
        }

        public int Length
        {
            get { return (int)wfr.Length; }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            wfr.Read(buffer, offset, count);
        }

        public void Dispose()
        {
            wfr.Dispose();
        }

        public WaveFormat WaveFormat { get { return wfr.WaveFormat; } }

    }
}
