using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace ll_synthesizer.Sound
{
    class AudioReader : SoundFileReader
    {
        private AudioFileReader afr;

        public AudioReader(string path)
        {
            afr = new AudioFileReader(path);
        }

        public int Length
        {
            get { return (int)afr.Length; }
        }

        public int Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public WaveFormat WaveFormat
        {
             get { return afr.WaveFormat; }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Read(ref byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
